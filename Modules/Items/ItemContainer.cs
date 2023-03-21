namespace queen.items;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using queen.error;

public partial class ItemContainer : Resource
{

    public SortedDictionary<Vector2I, ItemStack> items = new(new ItemContainerComparer(Vector2I.Zero));
    public event Action? MarkDirty;

    private Vector2I _containerSize = new Vector2I(1, 1);
    public Vector2I ContainerSize
    {
        get { return _containerSize; }
        set
        {
            _containerSize = value;
            if (items.Comparer is ItemContainerComparer icc)
                icc.Size = _containerSize;
        }
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
            // Print.Debug($"Removed From Stack: {stack} Qty {toRemove}");
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

    public List<ItemStack> GetAllOfType(string ItemKey)
    {
        return items.Values.Where(x => x.StackItem.ItemKey == ItemKey).ToList();
    }

    public bool RemoveAt(Vector2I position)
    {
        if (items.ContainsKey(position))
        {
            // // Print.Debug($"Removed Stack: {items[position]} @ {position}");
            items.Remove(position);
            return false;
        }
        var list = new List<Vector2I>();
        list.AddRange(items.Keys); // protected from modification -> prevents accidental deletion of
        foreach (var entry in list)
        {
            var rect = GetRectFor(entry);
            if (SlotPosInRect(position, rect))
            {
                // // Print.Debug($"Removed Stack: {items[entry]} @ {entry}");
                items.Remove(entry);
            }
        }
        DoMarkDirty();
        return true;
    }

    public bool CanPlaceAt(ItemStack stack, Vector2I target)
    {
        if (items.ContainsKey(target)) return false;
        // if there are no collisions, we can place at position
        if (stack is null || stack.StackItem is null)
        {
            Print.Warn($"Attempted to place a null stack at position in container: {target}");
            return false;
        }
        return !CheckForCollision(target, stack.StackItem.ItemSize, out _);
    }

    public bool PlaceAt(ItemStack stack, Vector2I target)
    {
        if (!CanPlaceAt(stack, target)) return false;
        items.Add(target, stack);
        // Print.Debug($"Placed Stack: {stack} @ {target}");
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
        var checkPos = new Vector2I();
        for (int y = 0; y < ContainerSize.Y; y++)
        {
            checkPos.Y = y;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                checkPos.X = x;
                var n_stack = stack.Copy();
                if (n_stack.StackCount > n_stack.MaxStackSize)
                    n_stack.StackCount = n_stack.MaxStackSize;

                if (CanPlaceAt(n_stack, checkPos))
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
            // // Print.Debug($"Calculating available stacks.\nStack Count={itemStack.StackCount}\nMax Stack Size={itemStack.MaxStackSize}\nQty Adding Total={stack.StackCount}\nQty Adding Here={adding}");
            itemStack.StackCount += adding;
            stack.StackCount -= adding;
            // Print.Debug($"Modified Stack: {itemStack} @ {entry.Key}");

            // // Print.Debug($"Added to existing stack: {stack.StackItem.ItemKey} X {adding}");

            if (stack.IsEmpty) return;
        }
    }

    private bool CheckForCollision(Vector2I position, Vector2I size, out Vector2I collisionPoint)
    {
        var rect = new Rect2I(position, size);
        collisionPoint = Vector2I.One * -1;
        // doesn't fit in slot because of container size
        if (!ContainerContains(rect))
        {
            // // Print.Debug($"Container Collision: Container Rect does not enclose item rect.\nContainer: {containerRect}\nItem: {rect}");
            return true;
        }

        foreach (var entry in items)
        {
            var target = GetRectFor(entry.Key);
            collisionPoint = entry.Key;
            // Found a collision against existing item
            if (RectCollision(target, rect))
            {
                // // Print.Debug($"Container Collision: Found Item Collision.\nItem A: {rect}\nItem B: {target}");
                return true;
            }
        }
        // // Print.Debug("Container Collision: No collisions found");
        collisionPoint = Vector2I.One * 420; // probably never going to have a 420x420 item container.
        return false;
    }

    private bool ContainerContains(Rect2I itemRect)
    {
        var rect = GetContainerRect();
        return RectCollision(itemRect, rect);
    }

    private bool RectCollision(Rect2I a, Rect2I b)
    {
        return a.Intersects(b);
        // return a.Position >= b.Position && (a.Position + a.Size) <= (b.Position + b.Size);
    }

    private bool SlotPosInRect(Vector2I slotPos, Rect2I itemRect)
    {
        return RectCollision(new Rect2I(slotPos, Vector2I.One), itemRect);
    }

    private Rect2I GetRectFor(Vector2I position)
    {
        var stack = GetAt(position);
        if (stack is null) return new Rect2I(position, Vector2I.Zero); // zero size rect, no collision should trigger
        return new Rect2I(position, stack.StackItem.ItemSize);
    }

    private Rect2I GetContainerRect()
    {
        return new Rect2I(Vector2I.Zero, ContainerSize);
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
            var key = GetIndexFrom(entry.Key);
            var stackData = entry.Value.SaveStack();
            data.Add(key, stackData);
        }
        return data;
    }

    public void LoadFromSaveData(Godot.Collections.Dictionary data)
    {
        items.Clear();
        var size = new Vector2I();
        size.X = data["size_x"].AsInt32();
        size.Y = data["size_y"].AsInt32();
        ContainerSize = size;
        data.Remove("size_x");
        data.Remove("size_y");
        foreach (var entry in data)
        {
            var position = GetFromIndex(entry.Key.AsInt32());
            var stackData = entry.Value.AsGodotDictionary();
            var stack = new ItemStack();
            stack.LoadStack(stackData);
            if (!stack.IsValidStack()) continue; // Discard data, not a useful value
            items.Add(position, stack);
        }
        DoMarkDirty();
    }

    private Vector2I GetFromIndex(int index)
    {
        return new Vector2I(index % ContainerSize.X, index / ContainerSize.X);
    }

    private int GetIndexFrom(Vector2I vector)
    {
        return vector.X + (vector.Y * ContainerSize.X);
    }

    private void DoMarkDirty() => MarkDirty?.Invoke();

    public ItemStack GetAt(Vector2I position)
    {
        if (items.ContainsKey(position)) return items[position];
        foreach (var entry in items.Keys)
        {
            var rect = GetRectFor(entry);
            if (SlotPosInRect(position, rect))
                return items[entry];
        }
        return null;
    }
}
