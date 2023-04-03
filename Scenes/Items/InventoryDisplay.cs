using System;
using Godot;
using queen.error;
using queen.events;
using queen.extension;
using queen.items;

public partial class InventoryDisplay : Control
{
    [Export] private NodePath PathPlayerInventoryDisplay;
    [Export] private NodePath PathSecondaryInventoryDisplay;

    private ItemsDisplayGeneric PlayerInventoryDisplay;
    private ItemsDisplayGeneric SecondaryInventoryDisplay;

    public override void _Ready()
    {
        this.GetSafe(PathPlayerInventoryDisplay, out PlayerInventoryDisplay);
        this.GetSafe(PathSecondaryInventoryDisplay, out SecondaryInventoryDisplay);
        Visible = false;
        Events.GUI.RequestInventory += OnInventoryRequested;
    }

    public override void _ExitTree()
    {
        Events.GUI.RequestInventory -= OnInventoryRequested;
    }

    private void OnInventoryRequested(object? PlayerInventory, object? SecondaryInventory)
    {
        Print.Debug($"InventoryRequest: {PlayerInventory}, {SecondaryInventory}");
        if (Visible || PlayerInventory is not ItemContainer PlayerItemContainer)
        {
            Visible = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
            return;
        }

        if (SecondaryInventory is ItemContainer SecondaryItemContainer)
        {
            SecondaryInventoryDisplay.Visible = true;
            SecondaryInventoryDisplay.SetContainer(SecondaryItemContainer);
            SecondaryInventoryDisplay.ReloadDisplay();
        }
        else
        {
            SecondaryInventoryDisplay.Visible = false;
        }

        PlayerInventoryDisplay.Visible = true;
        PlayerInventoryDisplay.SetContainer(PlayerItemContainer);
        PlayerInventoryDisplay.ReloadDisplay();
        Visible = true;

        var middle = GetViewport().GetVisibleRect().Size / 2.0f;
        Input.WarpMouse(middle);
        Input.MouseMode = Input.MouseModeEnum.Visible;

    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible) return;

        if (e.IsActionPressed("open_inventory"))
        {
            Events.GUI.TriggerRequestInventory(null, null);
            this.HandleInput();
        }
    }
}
