using System;
using Godot;
using Godot.Collections;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Item))]
public partial class Item : Resource
{

    [Export] public string ItemName = "unnamed";
    [Export] public string ItemDescription = "no description";
    [Export] public PackedScene ItemWorldMesh;
    [Export] public Texture ItemInventoryIcon;
    [Export] public Vector2I ItemInventorySize = new(1, 1);
    [Export] public bool IsEquippable = false;
    [Export] public int MaxStackSize = 99;
    [Export] public bool HasCustomData = false;

    public virtual Dictionary GetCustomData()
    {
        return null;
    }

    public virtual void LoadCustomData(Dictionary data) { }

}
