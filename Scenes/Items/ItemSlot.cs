using System;
using Godot;
using queen.extension;
using queen.items;

public partial class ItemSlot : TextureRect
{
    [Export] private Texture2D textureSlotDefault;
    [Export] private Texture2D textureSlotLocked;
    [Export] private Material slotMaterial;

    public bool IsLocked { get; private set; } = false;

    public ItemContainer ItemContainerOwner { get; private set; } = null;
    public Vector2I ContainerSlotPosition { get; private set; } = Vector2I.Zero;

    public override void _Ready()
    {
        Material = slotMaterial;
        Texture = GetSlotTexture();
    }

    public void SetItemContainerOwner(ItemContainer container, Vector2I slotPosition)
    {
        ItemContainerOwner = container;
        ContainerSlotPosition = slotPosition;
    }

    private Texture2D GetSlotTexture()
    {
        return IsLocked ? textureSlotLocked : textureSlotDefault;
    }


}
