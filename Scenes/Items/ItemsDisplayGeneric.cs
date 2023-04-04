using System;
using System.Threading.Tasks;
using Godot;
using queen.data;
using queen.error;
using queen.extension;
using queen.items;
using queen.math;

public partial class ItemsDisplayGeneric : Control
{

    [Export] private NodePath PathSlotsContainer;
    [Export] private NodePath PathItemIconsContainer;
    [Export] private PackedScene ItemSlotScene;
    [Export] private PackedScene ItemIconScene;
    [Export] private float ItemDragLerpWeight = 0.003f;

    private ItemContainer container;
    private GridContainer SlotsContainer;
    private Control ItemIconsContainer;

    // private static bool ReadyForDragDrop = true;

    private struct DraggingInfoData
    {
        public Vector2 OriginalPosition;
        public Vector2 MouseDownPosition;
        public TextureRect? DragObject;
        public ItemStack MovingStack;
        public bool IsDragging;

        public ItemContainer OriginalContainer;
        public InventoryPosition PositionInOriginalContainer;

    }
    private static DraggingInfoData DragInfo; // All displays share a single drag object

    // private Vector2 SlotOffsetTolerance = (Vector2.One * -32f);

    public override void _Ready()
    {
        this.GetSafe(PathSlotsContainer, out SlotsContainer);
        this.GetSafe(PathItemIconsContainer, out ItemIconsContainer);
        DragInfo.IsDragging = false;
    }


    public void SetContainer(ItemContainer n_container)
    {
        // if (container is not null) container.MarkDirty -= ReloadDisplay;
        container = n_container;
        if (container is not null) container.MarkDirty += ReloadDisplay;
        ReloadDisplay();
    }

    public void ReloadDisplay()
    {
        SlotsContainer.RemoveAllChildren();
        ItemIconsContainer.RemoveAllChildren();

        if (container is null) return;
        // Create slots
        var size = container.ContainerSize;
        if (size.X <= 0)
        {
            Print.Warn("Container Size was less than or equal to zero. This doesn't work for grid containers!");
            return;
        }
        SlotsContainer.Columns = size.X;
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                var slot = (ItemSlot)ItemSlotScene.Instantiate();
                SlotsContainer.AddChild(slot);
                var pos = new InventoryPosition(x, y);
                slot.SetItemContainerOwner(container, pos);
                if (container.CheckForCollision(pos, InventoryPosition.One, out _))
                {
                    slot.Modulate = Colors.RebeccaPurple;
                }
                var stack = container.GetAt(pos, InventoryPosition.One);
                if (stack is not null && !stack.IsValidStack())
                {
                    slot.Modulate = Colors.Orange;

                    if (stack.StackCount < 0)
                    {
                        Print.Warn("Found an item with a negative count!");
                        slot.Modulate = Colors.SlateGray;
                    }
                }

            }
        }

        // Create Item Icons
        foreach (var entry in container.items)
        {
            var position = entry.Key;
            var slot = SlotsContainer.GetChildOrNull<TextureRect>(position);
            if (slot is null)
            {
                Print.Error("Probably should never happen. Check in on this immediately.");
                continue;
            }
            var item = entry.Value.StackItem;
            if (item is null) continue;

            var icon = (ItemIcon)ItemIconScene.Instantiate();

            ItemIconsContainer.AddChild(icon);
            icon.UpdateIcon(entry.Value);
            icon.Size = slot.GetMinimumSize() * item.ItemSize;
            icon.Position = slot.GetMinimumSize() * InventoryPosition.FromIndex(position, container.ContainerSize.X).ToVector2I();
        }
    }

    public override void _Process(double delta)
    {
        if (!DragInfo.IsDragging) return;

        if (DragInfo.DragObject is null || !IsInstanceValid(DragInfo.DragObject)) return;
        DragInfo.DragObject.GlobalPosition = GetGlobalMousePosition() + new Vector2(-32, -32);
        DragInfo.DragObject.Modulate = Colors.Green;
    }

    public override void _Input(InputEvent e)
    {
        if (!Visible) return;
        // if (!ReadyForDragDrop) return;

        if (DragInfo.IsDragging && e.IsActionReleased("pickup_item"))
        {
            var position = GetGlobalMousePosition();
            if (GetGlobalRect().HasPoint(position) && (DragInfo.MouseDownPosition - position).Length() > 32.0)
            {
                // this is checking that we released on this display, we are aleady dragging, and the mouse has moved at least 32 pixels from the down position, which likely is a different slot. So clicking without moving the mouse is a toggle, but click and drag still operates as expected
                if (DragInfo.IsDragging && DragInfo.DragObject != null)
                {
                    Print.Info("Attempting Dragged Drop");
                    TryDropItem(position);
                }
            }
        }

        if (e.IsActionPressed("pickup_item"))
        {
            var position = GetGlobalMousePosition();
            // TODO create a virtual cursor for gamepad support
            DragInfo.MouseDownPosition = position;

            if (GetGlobalRect().HasPoint(position))
            {
                if (DragInfo.IsDragging && DragInfo.DragObject != null)
                {
                    Print.Info("Attempting Toggled Drop");
                    TryDropItem(position);
                }
                else
                {
                    TryStartDrag(position);
                }
            }
        }
    }

    public void TryStartDrag(Vector2 atPosition)
    {
        if (!GetGlobalRect().HasPoint(atPosition)) return;
        ItemSlot? slot = TryFindSlotFor(atPosition);
        if (slot is null) return;

        var children = ItemIconsContainer.GetChildren();
        foreach (var child in children)
        {
            if (child is not ItemIcon icon) continue;
            if (!icon.GetGlobalRect().HasPoint(atPosition)) continue;

            // we have a hit
            DragInfo.DragObject = icon;
            DragInfo.OriginalPosition = icon.GlobalPosition;
        }


        DragInfo.OriginalContainer = slot.ItemContainerOwner;
        DragInfo.PositionInOriginalContainer = slot.ContainerSlotPosition;
        var stack = slot.ItemContainerOwner.GetAt(slot.ContainerSlotPosition, InventoryPosition.One);
        if (stack is not null && stack.IsValidStack() && DragInfo.DragObject is not null)
        {
            DragInfo.MovingStack = stack;
            DragInfo.DragObject.Modulate = Colors.Red;
            DragInfo.DragObject.ZIndex = 128;
            DragInfo.IsDragging = true;
        }
        else { Print.Warn($"failed to find item stack from slot: {slot.ContainerSlotPosition}"); }
    }

    private void TryDropItem(Vector2 position)
    {
        // TODO : Somewhere from this function we are allowing item duplication!
        if (!DragInfo.IsDragging) return;
        if (DragInfo.DragObject is null) return;


        // place at appropriate position
        DragInfo.DragObject.ZIndex = 0;
        DragInfo.IsDragging = false;
        DragInfo.DragObject = null;

        //
        ItemSlot? slotTarget = TryFindSlotFor(position);
        if (slotTarget is not null)
        {
            if (slotTarget.ItemContainerOwner != DragInfo.OriginalContainer)
            {
                if (!slotTarget.ItemContainerOwner.CanPlaceAt(DragInfo.MovingStack, slotTarget.ContainerSlotPosition))
                {
                    // specifically don't allow swapping between containers to avoid some edge cases. Maybe I'll make this better someday
                    DragInfo.OriginalContainer.DoMarkDirty();
                    ReloadDisplay();
                    return;
                }
                // TODO For some reason this isn't gelling quite well
                // manual swap
                // remove original drag
                DragInfo.OriginalContainer.RemoveAt(DragInfo.PositionInOriginalContainer);
                // Place in new container
                slotTarget.ItemContainerOwner.PlaceAt(DragInfo.MovingStack, slotTarget.ContainerSlotPosition);
                DragInfo.OriginalContainer.DoMarkDirty();
            }
            else
            {
                // internal swap
                slotTarget.ItemContainerOwner.TrySwapSlots(DragInfo.PositionInOriginalContainer, slotTarget.ContainerSlotPosition);
            }
        }

        ReloadDisplay();
    }

    private ItemSlot? TryFindSlotFor(Vector2 mouse_position)
    {
        var children = SlotsContainer.GetChildren();
        foreach (var child in children)
        {
            if (child is not ItemSlot slot) continue;
            if (!slot.GetGlobalRect().HasPoint(mouse_position)) continue;
            // we have a hit
            return slot;
        }
        return null;
    }

    private void SaveInventory()
    {
        var data = container.GetSaveData();
        var stringData = Json.Stringify(data, "\t");
        Data.CurrentSaveSlot.SaveText(stringData, $"{Name}_inventory.json");
    }

    private void LoadInventory()
    {
        var stringData = Data.CurrentSaveSlot.LoadText($"{Name}_inventory.json", false);
        var data = Json.ParseString(stringData).AsGodotDictionary();
        if (data is not null) container.LoadFromSaveData(data);
        CallDeferred(nameof(ReloadDisplay));
    }
}
