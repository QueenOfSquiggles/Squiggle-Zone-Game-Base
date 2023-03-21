using System;
using System.Threading.Tasks;
using Godot;
using queen.data;
using queen.error;
using queen.extension;
using queen.items;

public partial class ItemsDisplayGeneric : Control
{


    [Export] private NodePath PathSlotsContainer;
    [Export] private NodePath PathItemIconsContainer;
    [Export] private PackedScene ItemSlotScene;
    [Export] private PackedScene ItemIconScene;
    [Export] private float ItemDragLerpWeight = 0.003f;

    private ItemContainer container = null;
    private GridContainer SlotsContainer;
    private Control ItemIconsContainer;

    // private static bool ReadyForDragDrop = true;

    private struct DraggingInfoData
    {
        public Vector2 OriginalPosition;
        public Control? DragObject;
        public ItemStack MovingStack;
        public bool IsDragging;

        public ItemContainer OriginalContainer;
        public Vector2I PositionInOriginalContainer;

    }
    private static DraggingInfoData DragInfo; // All displays share a single drag object

    // private Vector2 SlotOffsetTolerance = (Vector2.One * -32f);

    public override void _Ready()
    {
        this.GetSafe(PathSlotsContainer, out SlotsContainer);
        this.GetSafe(PathItemIconsContainer, out ItemIconsContainer);
        DragInfo.IsDragging = false;

#if DEBUG
        TestContainers();
#endif
    }

    private void TestContainers()
    {
        Print.Warn("Warning. Testing item display should only ever happen when testing. Remove when done!!!");

        ItemContainer ic = new()
        {
            ContainerSize = new(5, 6)
        };
        AddItem(ref ic, "base.item.test", 1);
        AddItem(ref ic, "base.item.diamond", 3);
        AddItem(ref ic, "base.item.gold_nugget", 3);
        SetContainer(ic);

        Print.Debug($"Items Dict:");
        foreach (var entry in ic.items)
        {
            Print.Debug($"\t{entry.Key} = {entry.Value}");
        }
    }

    private void AddItem(ref ItemContainer container, string ItemKey, int count)
    {
        ItemStack stack = new(ItemRegistry.GetItem(ItemKey), count);
        container.AddStack(ref stack);
    }


    public void SetContainer(ItemContainer n_container)
    {
        if (container is not null) container.MarkDirty -= ReloadDisplay;
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
        SlotsContainer.Columns = size.X;

        for (int i = 0; i < size.X * size.Y; i++)
        {
            var slot = (ItemSlot)ItemSlotScene.Instantiate();
            SlotsContainer.AddChild(slot);
            slot.SetItemContainerOwner(container, new Vector2I(i % size.X, i / size.X));
        }

        // Create Item Icons
        foreach (var entry in container.items)
        {
            var position = entry.Key;
            int childIndex = position.X + (position.Y * container.ContainerSize.X);
            var slot = SlotsContainer.GetChildOrNull<TextureRect>(childIndex);
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
            icon.Position = slot.GetMinimumSize() * position;
        }
    }

    public override void _Process(double delta)
    {
        // Print.Debug("Process Function");
        if (!DragInfo.IsDragging) return;

        // Print.Debug("Dragging Item Icon!");
        if (DragInfo.DragObject is null) return;
        DragInfo.DragObject.GlobalPosition = GetGlobalMousePosition() + DragInfo.DragObject.Size * -0.5f;
        DragInfo.DragObject.Modulate = Colors.Green;
    }

    public override void _Input(InputEvent e)
    {
        if (!Visible) return;
        // if (!ReadyForDragDrop) return;

        if (e.IsActionPressed("pickup_item"))
        {
            // Print.Debug("Drag/Drop Triggered");
            var position = GetGlobalMousePosition();
            // TODO create a virtual cursor for gamepad support

            if (GetGlobalRect().HasPoint(position))
            {
                if (DragInfo.IsDragging && DragInfo.DragObject != null)
                    TryDropItem(position);
                else
                    TryStartDrag(position);
                SafetyDelay();
            }
        }



        if (e is InputEventKey key && key.Pressed)
        {
            if (key.Keycode == Key.KpAdd)
                AddItem(ref container, "base.item.gold_nugget", 1);
            if (key.Keycode == Key.KpSubtract)
                AddItem(ref container, "base.item.diamond", 1);
            if (key.Keycode == Key.Kp0)
            {
                var stack = new ItemStack(ItemRegistry.GetItem("base.item.gold_nugget"), 1);
                container.RemoveStack(stack);
            }
            if (key.Keycode == Key.Kp1)
            {
                var stack = new ItemStack(ItemRegistry.GetItem("base.item.diamond"), 1);
                container.RemoveStack(stack);
            }
        }
    }

    private async void SafetyDelay()
    {
        // effectively a false mutex that protects from random double click
        // ReadyForDragDrop = false;
        await Task.Delay(200);
        // ReadyForDragDrop = true;
    }

    public void TryStartDrag(Vector2 atPosition)
    {
        // Print.Debug("TryStartDrag()");
        if (!GetGlobalRect().HasPoint(atPosition))
        {
            Print.Debug($"Failed to interact with inventory '{Name}'. Point is not in rect {atPosition}");
            return;
        }



        //Print.Debug($"Trying to start drag on inventory: {Name}");
        var children = ItemIconsContainer.GetChildren();
        foreach (var child in children)
        {
            if (child is not ItemIcon icon) continue;
            if (!icon.GetGlobalRect().HasPoint(atPosition)) continue;
            // we have a hit
            DragInfo.DragObject = icon;
            DragInfo.OriginalPosition = icon.GlobalPosition;
            bool FoundSlot = false;
            var SlotsList = SlotsContainer.GetChildren();
            foreach (var slotChild in SlotsList)
            {
                if (slotChild is not ItemSlot slot) continue;
                if (!slot.GetGlobalRect().HasPoint(atPosition)) continue;

                DragInfo.OriginalContainer = slot.ItemContainerOwner;
                DragInfo.PositionInOriginalContainer = slot.ContainerSlotPosition;
                DragInfo.MovingStack = slot.ItemContainerOwner.GetAt(slot.ContainerSlotPosition);

                if (DragInfo.MovingStack == null || DragInfo.MovingStack.StackItem == null)
                {
                    Print.Warn($"failed to find item stack from slot: {slot.ContainerSlotPosition}");
                }
                else
                {
                    FoundSlot = true;
                }
                break;
            }
            if (FoundSlot)
            {
                DragInfo.DragObject.Modulate = Colors.Red;
                DragInfo.DragObject.ZIndex = 128;
                DragInfo.IsDragging = true;
                // SlotOffsetTolerance = -(DragInfo.DragObject.Size * 0.45f); // slightly less than half
                //                container.RemoveAt(DragInfo.PositionInOriginalContainer);
                // Print.Debug("Starting drag");
                break;
            }
        }
    }

    private void TryDropItem(Vector2 position)
    {
        // Print.Debug("TryDropItem()");
        //if (!GetGlobalRect().HasPoint(position)) return;
        if (DragInfo.DragObject is null) return;
        DragInfo.OriginalContainer.RemoveAt(DragInfo.PositionInOriginalContainer);

        var children = SlotsContainer.GetChildren();
        ItemSlot? slotTarget = null;
        foreach (var child in children)
        {
            if (child is not ItemSlot slot) continue;
            if (!slot.GetGlobalRect().HasPoint(position)) continue;
            // we have a hit
            var targetContainer = slot.ItemContainerOwner;
            if (targetContainer.CanPlaceAt(DragInfo.MovingStack, slot.ContainerSlotPosition))
            {
                slotTarget = slot;
            }
        }
        if (slotTarget is null)
        {
            // Return to original position
            var success = DragInfo.OriginalContainer.PlaceAt(DragInfo.MovingStack, DragInfo.PositionInOriginalContainer);
            if (!success)
            {
                var stack = DragInfo.MovingStack.Copy();
                DragInfo.OriginalContainer.AddStack(ref stack); // add to any available slot when fail
            }
        }
        else
        {
            slotTarget.ItemContainerOwner.PlaceAt(DragInfo.MovingStack, slotTarget.ContainerSlotPosition);
            DragInfo.DragObject.ZIndex = 0;
            if (DragInfo.OriginalContainer != slotTarget.ItemContainerOwner)
            {
                DragInfo.DragObject.QueueFree();
            }
            ReloadDisplay();
        }
        DragInfo.IsDragging = false;
        DragInfo.DragObject = null;
        // SlotOffsetTolerance = Vector2I.Zero; // slightly less than half
    }

    private void SaveInventory()
    {
        var data = container.GetSaveData();
        var stringData = Json.Stringify(data, "\t");
        Data.CurrentSaveSlot.SaveText(stringData, $"{Name}_inventory.json");
    }

    private void LoadInventory()
    {
        var stringData = Data.CurrentSaveSlot.LoadText($"{Name}_inventory.json");
        var data = Json.ParseString(stringData).AsGodotDictionary();
        if (data is not null) container.LoadFromSaveData(data);
        CallDeferred(nameof(ReloadDisplay));
    }
}
