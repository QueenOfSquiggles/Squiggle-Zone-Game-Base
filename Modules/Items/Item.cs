namespace queen.items;

using System;
using Godot;
using Godot.Collections;
using MonoCustomResourceRegistry;
using queen.extension;
using queen.math;

/// <summary>
/// The definitive data values for a given Item. No instance data is stored here. For instance data storage See <seealso cref="ItemStack"/>
/// </summary>
[RegisteredType(nameof(Item))]
public partial class Item : Resource
{
    [Export] public string ItemKey { get; set; } = "item.null";
    [Export] private Texture2D? ItemTexture = null;
    [Export] public Vector2I ItemSize { get; set; } = new Vector2I(1, 1);
    [Export] public int MaxStackSize { get; set; } = 1;
    [Export] public Mesh? ItemMesh { get; set; } = null;
    public bool IsStackable => MaxStackSize > 1;

    public virtual bool HasCustomData => false;

    /// <summary>
    /// Build the instance data to be saved when serializing an item stack of this item
    /// </summary>
    /// <param name="stack">the instance of the stack that contains one or more of this item</param>
    /// <returns>A Godot Dictionary that stores the information for loading custom instance data next time.</returns>
    public virtual Dictionary SaveInstanceData(ItemStack stack) => new Dictionary();

    /// <summary>
    /// Gets the texture of the item. This is a virtual method so that special items can have a custom texture based on data.
    /// </summary>
    /// <returns>The Texture2D that is currently the texture</returns>
    public virtual Texture2D? GetItemTexture() => ItemTexture;

    public override bool Equals(object? obj)
    {
        if (obj is Item other)
        {
            return other.ItemKey == ItemKey;
        }
        return false;
    }

    public override string ToString()
    {
        return $"Item<{ItemKey}>";
    }

    public override int GetHashCode()
    {
        return (int)ItemKey.Hash();
    }

    public InventoryPosition GetSizeAsPos()
    {
        return ItemSize.ToInventoryPosition();
    }
}
