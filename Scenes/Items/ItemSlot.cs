using System;
using Godot;
using queen.extension;
using queen.items;
using queen.math;

public partial class ItemSlot : TextureRect
{
    [Export] private Texture2D textureSlotDefault;
    [Export] private Texture2D textureSlotLocked;
    [Export] private Material slotMaterial;
    [Export] private NodePath PathDebugLabel;
    [Export] private bool Testing = false;

    public bool IsLocked { get; private set; } = false;

    public ItemContainer ItemContainerOwner { get; private set; } = null;
    public InventoryPosition ContainerSlotPosition { get; private set; } = InventoryPosition.Zero;
    private Label DebugLabel;

    public override void _Ready()
    {
        Material = slotMaterial;
        Texture = GetSlotTexture();
        this.GetSafe(PathDebugLabel, out DebugLabel);
        DebugLabel.Text = "";
    }

    public void SetItemContainerOwner(ItemContainer container, InventoryPosition slotPosition)
    {
        ItemContainerOwner = container;
        ContainerSlotPosition = slotPosition;
#if DEBUG
        if (Testing)
        { // if we are in a debug environment and specifically enabling slot debugging, show the label.
            DebugLabel.Text = $"({slotPosition.X}, {slotPosition.Y})";
        }
#endif

    }

    private Texture2D GetSlotTexture()
    {
        return IsLocked ? textureSlotLocked : textureSlotDefault;
    }


}
