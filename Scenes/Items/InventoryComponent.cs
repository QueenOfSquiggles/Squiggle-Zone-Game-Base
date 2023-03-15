using System;
using Godot;

public partial class InventoryComponent : Node
{

    /// <summary>
    /// The ItemContainer to treat as the inventory
    /// </summary>
    [Export] public ItemContainer InventoryContainer;

}
