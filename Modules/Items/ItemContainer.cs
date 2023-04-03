namespace queen.items;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using queen.error;
using queen.extension;
using queen.math;

public partial class ItemContainer : Resource
{
    public SortedDictionary<int, ItemStack> items = new();
    public event Action? MarkDirty;

    // private Vec2i _containerSize = new Vec2i(1, 1);
    public InventoryPosition ContainerSize { get; private set; } = new();


    /// <summary>
    /// Resizes the item container. Popping out any items that no longer fit
    /// </summary>
    /// <param name="new_size">The new size of the container</param>
    /// <param name="popped_items">A list of item stacks that are now being popped out of the container</param>
    /// <returns>True if the change was successful, false if failing. Usually failure means an invalid container size was passed</returns>
    public bool ResizeContainer(InventoryPosition new_size, out List<ItemStack> popped_items)
    {
        popped_items = new();
        if (new_size <= InventoryPosition.Zero) return false;
        ContainerSize = new_size;
        var tempDict = new Dictionary<InventoryPosition, ItemStack>();

        // Three consecutive for loops is pretty gross. Is there possibly a better way to do this little truffle shuffle? TBF this should not be run very often so being a little less performant isn't too big of a deal, so long as it doesn't cause a snag in the run

        foreach (var entry in items)
        {
            if (CheckForCollision(InventoryPosition.FromIndex(entry.Key, ContainerSize.X), entry.Value.StackItem.ItemSize.ToInventoryPosition(), out _))
            {
                tempDict.Add(InventoryPosition.FromIndex(entry.Key, ContainerSize.X), entry.Value);
            }
        }
        foreach (var entry in tempDict)
        { // items queued for removal
            items.Remove(entry.Key.ToIndex(ContainerSize.X));
        }
        foreach (var entry in tempDict)
        { // Try to add back anything in the remaining slots
            var toAdd = entry.Value.Copy();
            if (!(AddStack(ref toAdd) && toAdd.IsEmpty))
            { // failed to add stack, pop remaining
                popped_items.Add(toAdd);
            }
        }
        return true;
    }


    public bool HasQuantity(ItemStack stack)
    {
        int remaining = stack.StackCount;
        foreach (var item in items.Values)
        {
            if (!stack.SameItem(item)) continue;
            remaining -= item.StackCount;
            if (remaining <= 0) return true;
        }
        return false;
    }

    public bool Consume(ItemStack stack) => RemoveStack(stack); // alternative name for clearer interpretation

    public bool RemoveStack(ItemStack stack)
    {
        if (!HasQuantity(stack)) return false;
        var matchingStacks = GetAllOfType(stack.StackItem.ItemKey);
        foreach (var itemStack in matchingStacks)
        {
            var toRemove = Math.Min(stack.StackCount, itemStack.StackCount);
            itemStack.StackCount -= toRemove;
            stack.StackCount -= toRemove;
            if (stack.IsEmpty) break;
        }
        CombineStacks();
        RemoveAllEmpty();
        DoMarkDirty();
        return stack.IsEmpty;
    }

    public void RemoveAllEmpty()
    {
        // Checks against the IsValidStack function to see if the stack is valid. Anything invalid gets cleared  
        var targets = items.Where(x => !x.Value.IsValidStack()).ToList();
        foreach (var t in targets)
        {
            items.Remove(t.Key);
        }
        DoMarkDirty();
    }

    public void CombineStacks()
    {
        List<Item> item_types = new();
        foreach (var entry in items.Values)
        {
            if (item_types.Contains(entry.StackItem)) continue;
            item_types.Add(entry.StackItem);
        }

        foreach (var item in item_types)
        {
            var list = GetAllOfType(item.ItemKey);
            int endIndex = list.Count - 1;
            for (int startIndex = 0; startIndex < endIndex; startIndex++)
            {
                var stack = list[startIndex];
                if (stack.IsFull) continue;
                var pullStack = list[endIndex];
                if (pullStack.IsEmpty)
                {
                    endIndex--; // go to next pull target, will break loop if hitting startIndex
                }
                else
                {
                    int moveQty = Math.Min(stack.MaxStackSize - stack.StackCount, pullStack.StackCount);
                    stack.StackCount += moveQty;
                    pullStack.StackCount -= moveQty;
                }
            }
        }
    }


    public bool TrySwapSlots(InventoryPosition slotA, InventoryPosition slotB)
    {
        if (slotA == slotB) return true; // Swapping from and to the same slot, success without doing anything 
        var stackA = GetAt(slotA, InventoryPosition.One);
        var stackB = GetAt(slotB, InventoryPosition.One);
        var deleteSlotA = slotA;
        var deleteSlotB = slotB;
        if (stackA is not null && stackB is null)
        {
            var hits = GetCollidingStacks(slotB, stackA.StackItem.ItemSize.ToInventoryPosition());
            if (hits.Count > 0)
            {
                var hit = hits[0];
                stackB = hit.Value;
                deleteSlotB = hit.Key;
            }
        }
        if (stackB is not null && stackA is null)
        {
            var hits = GetCollidingStacks(slotA, stackB.StackItem.ItemSize.ToInventoryPosition());
            if (hits.Count > 0)
            {
                var hit = hits[0];
                stackB = hit.Value;
                deleteSlotB = hit.Key;
            }
        }

        { // debugging chunk
            var lblStackA = stackA == null ? "null" : stackA.ToString();
            var lblStackB = stackB == null ? "null" : stackB.ToString();
        }

        if (stackB is null || stackA is null)
        {
            return HandleSwapPlacementForNull(slotA, stackA, slotB, stackB);
        }
        return HandleTrueSwapPlacement(slotA, stackA, slotB, stackB, deleteSlotA, deleteSlotB);
    }

    private bool HandleSwapPlacementForNull(InventoryPosition slotA, ItemStack? stackA, InventoryPosition slotB, ItemStack? stackB)
    {
        if (stackA is null && stackB is null) return true; // success because there is nothing to move
        if (stackA is null && stackB is not null)
        {
            // place stackB at slotA
            RemoveAt(slotB);
            return PlaceAt(stackB, slotA);
        }
        if (stackB is null && stackA is not null)
        {
            // place stackB at slotA
            RemoveAt(slotA);
            return PlaceAt(stackA, slotB);
        }
        return false;
    }

    private bool HandleTrueSwapPlacement(InventoryPosition slotA, ItemStack stackA, InventoryPosition slotB, ItemStack stackB, InventoryPosition deleteSlotA, InventoryPosition deleteSlotB)
    {
        bool success = true;
        // remove stacks first to avoid ghost collision
        success &= RemoveAt(deleteSlotA);
        success &= RemoveAt(deleteSlotB);
        var collisionsListA = GetCollidingStacks(slotA, stackA.StackItem.ItemSize.ToInventoryPosition());
        var collisionsListB = GetCollidingStacks(slotB, stackB.StackItem.ItemSize.ToInventoryPosition());

        if (CanPlaceAt(stackA, slotB) && CanPlaceAt(stackB, slotA) && collisionsListA.Count <= 0 && collisionsListB.Count <= 0)
        {
            // swap will succeed only if we are able to place the stack at both positions with no extra collisions.
            TryPlaceAt(ref stackA, slotB);
            TryPlaceAt(ref stackB, slotA);
        }
        else
        {
            // swap would fail, reset tro original positions
            success &= PlaceAt(stackA, slotA);
            success &= PlaceAt(stackB, slotB);
        }

        return success; // returns true if all swapping logic succeeded
    }

    public List<ItemStack> GetAllOfType(string ItemKey)
    {
        return items.Values.Where(x => x.StackItem.ItemKey == ItemKey).ToList();
    }

    public bool RemoveAt(InventoryPosition position)
    {
        var index = position.ToIndex(ContainerSize.X);
        if (items.ContainsKey(index))
        {
            items.Remove(position.ToIndex(ContainerSize.X));
            return false;
        }
        var list = new List<int>();
        list.AddRange(items.Keys); // protected from modification -> prevents accidental deletion of
        foreach (var entry in list)
        {
            var rect = GetRectFor(InventoryPosition.FromIndex(entry, ContainerSize.X));
            if (SlotPosInRect(position, rect))
            {
                items.Remove(entry);
                RemoveAllEmpty();
                DoMarkDirty();
                return true;
            }
        }
        return false;
    }

    public bool CanPlaceAt(ItemStack stack, InventoryPosition target)
    {
        var targetIndex = target.ToIndex(ContainerSize.X);
        if (items.ContainsKey(targetIndex))
        {
            var foundStack = items[targetIndex];
            Print.Warn($"Checking if CanPlace {stack} at {target} returned false. Key '{target}' already present, contains: {foundStack}");
            DebugItemContainerDump();
            return false;
        }
        // if there are no collisions, we can place at position
        if (stack is null || stack.StackItem is null)
        {
            Print.Warn($"Attempted to place a null stack at position in container: {target}");
            return false;
        }

        // check that the stack position fits in the container
        if (!ContainerContains(new Rect2I(target.ToVector2I(), stack.StackItem.ItemSize))) return false;

        return !CheckForCollision(target, stack.StackItem.ItemSize.ToInventoryPosition(), out _);
    }

    public void TryPlaceAt(ref ItemStack stack, InventoryPosition target)
    {
        // attempts to place at intended positon, falling back on adding to first available slot.
        if (!PlaceAt(stack, target)) AddStack(ref stack);
    }

    public bool PlaceAt(ItemStack stack, InventoryPosition target)
    {
        if (!CanPlaceAt(stack, target)) return false;
        items.Add(target.ToIndex(ContainerSize.X), stack);
        DoMarkDirty();
        return true;
    }

    public bool AddStack(ref ItemStack stack)
    {
        if (stack.IsStackable)
            AddToExistingStacks(ref stack);
        if (stack.StackCount > 0)
            AddNewStack(ref stack);
        DoMarkDirty();
        return stack.IsEmpty;
    }

    private void AddNewStack(ref ItemStack stack)
    {
        var checkPos = new InventoryPosition();
        for (int y = 0; y < ContainerSize.X; y++)
        {
            checkPos.Y = y;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                checkPos.X = x;
                var n_stack = stack.Copy();
                if (n_stack.StackCount > n_stack.MaxStackSize)
                    n_stack.StackCount = n_stack.MaxStackSize;

                if (!items.ContainsKey(checkPos.ToIndex(ContainerSize.X)) && CanPlaceAt(n_stack, checkPos))
                {
                    PlaceAt(n_stack, checkPos);
                    stack.StackCount -= n_stack.StackCount;
                    if (stack.StackCount <= 0) return;
                }
            }
        }
    }

    private void AddToExistingStacks(ref ItemStack stack)
    {
        foreach (var entry in items)
        {
            var itemStack = entry.Value;
            if (!itemStack.SameItem(stack)) continue;
            if (itemStack.IsFull) continue;

            int adding = Math.Min(itemStack.MaxStackSize - itemStack.StackCount, stack.StackCount);
            itemStack.StackCount += adding;
            stack.StackCount -= adding;
            if (stack.IsEmpty) return;
        }
    }

    public bool CheckForCollision(InventoryPosition position, InventoryPosition size, out InventoryPosition collisionPoint)
    {
        var rect = new Rect2I(position, size);
        collisionPoint = InventoryPosition.One * -1;
        foreach (var entry in items)
        {
            InventoryPosition vec = InventoryPosition.FromIndex(entry.Key, ContainerSize.X);
            var target = GetRectFor(vec);
            collisionPoint = vec;
            // Found a collision against existing item
            if (RectCollision(target, rect))
            {
                return true;
            }
        }
        collisionPoint = InventoryPosition.One * 420; // probably never going to have a 420x420 item container.
        return false;
    }

    private bool ContainerContains(Rect2I itemRect)
    {
        var rect = GetContainerRect();
        // check that top left and bottom right fit within the container's rect. Should solve containing larger items
        if (!RectCollision(itemRect, rect)) return false;
        var itemMax = itemRect.Position + itemRect.Size;
        var containerMax = rect.Size;
        // handle upper left corner
        if (itemRect.Position.X < 0) return false;
        if (itemRect.Position.Y < 0) return false;
        // handle bottom right corner
        if (itemMax.X > containerMax.X) return false;
        if (itemMax.Y > containerMax.Y) return false;

        return true;
    }

    private static bool RectCollision(Rect2I a, Rect2I b)
    {
        // return a.Intersects(b) || b.Intersects(a);
        // returns true if there is an overlap on both the X and Y axes. 
        return LineCollision(a.Position.X, a.Size.X, b.Position.X, b.Size.X) &&
        LineCollision(a.Position.Y, a.Size.Y, b.Position.Y, b.Size.Y);
    }

    private static bool LineCollision(int A_pos, int A_width, int B_pos, int B_width)
    {
        if (A_pos == B_pos) return true; // early out, checking against same position is always a collision
        if (A_pos < B_pos + B_width && A_pos >= B_pos) return true; // A in middle of B region
        if (B_pos < A_pos + A_width && B_pos >= A_pos) return true; // B in middle of A region 
        return false;
    }

    private static bool SlotPosInRect(InventoryPosition slotPos, Rect2I itemRect)
    {
        return RectCollision(new Rect2I(slotPos, InventoryPosition.One), itemRect);
    }

    private Rect2I GetRectFor(InventoryPosition position)
    {
        var stack = GetAt(position, InventoryPosition.One);
        if (stack is null) return new Rect2I(position, InventoryPosition.Zero); // zero size rect, no collision should trigger
        return new Rect2I(position, stack.StackItem.ItemSize);
    }

    private Rect2I GetContainerRect()
    {
        return new Rect2I(InventoryPosition.Zero, ContainerSize);
    }

    public Godot.Collections.Dictionary GetSaveData()
    {
        var data = new Godot.Collections.Dictionary()
        {
            {"size_x", ContainerSize.X},
            {"size_y", ContainerSize.Y}
        };

        foreach (var entry in items)
        {
            var stackData = entry.Value.SaveStack();
            data.Add(entry.Key, stackData);
        }
        return data;
    }

    public void LoadFromSaveData(Godot.Collections.Dictionary data)
    {
        items.Clear();
        var size = new InventoryPosition
        {
            X = data["size_x"].AsInt32(),
            Y = data["size_y"].AsInt32()
        };
        ResizeContainer(size, out _);

        data.Remove("size_x");
        data.Remove("size_y");
        foreach (var entry in data)
        {
            var index = entry.Key.AsInt32();
            var stackData = entry.Value.AsGodotDictionary();
            var stack = new ItemStack();
            stack.LoadStack(stackData);
            if (!stack.IsValidStack()) continue; // Discard data, not a useful value
            items.Add(index, stack);
        }
        DoMarkDirty();
    }

    public void DoMarkDirty() => MarkDirty?.Invoke();

    public ItemStack? GetAt(InventoryPosition position, InventoryPosition size)
    {
        var index = position.ToIndex(ContainerSize.X);
        if (items.ContainsKey(index)) return items[index];
        var hits = GetCollidingStacks(position, size);
        if (hits.Count <= 0) return null;
        return hits[0].Value;
    }

    public List<KeyValuePair<InventoryPosition, ItemStack>> GetCollidingStacks(InventoryPosition position, InventoryPosition itemSize)
    {
        var collisions = new List<KeyValuePair<InventoryPosition, ItemStack>>();
        var maskRect = new Rect2I(position.ToVector2I(), itemSize.ToVector2I());

        foreach (var itemEntry in items)
        {
            var itemPos = InventoryPosition.FromIndex(itemEntry.Key, ContainerSize.X);
            var itemRect = new Rect2I(itemPos, itemEntry.Value.StackItem.ItemSize.ToInventoryPosition());

            if (RectCollision(maskRect, itemRect))
            {
                collisions.Add(new(itemPos, itemEntry.Value));
            }
        }
        return collisions;
    }

    public void DebugItemContainerDump()
    {
        Print.Debug("---Begin Item Container Dump---");
        foreach (var entry in items)
        {
            var itemRect = GetRectFor(InventoryPosition.FromIndex(entry.Key, ContainerSize.X));
            Print.Debug($"({entry.Key})|- {entry.Value} w/ bounds [{itemRect}]");
        }
        Print.Debug("---End Item Container Dump---");
    }
}
