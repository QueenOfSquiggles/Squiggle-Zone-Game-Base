using System;
using Godot;
using queen.data;
using queen.error;
using queen.events;
using queen.extension;
using queen.items;

public partial class InventoryComponent : Node
{

    [Export] private Vector2I InventoryDefaultSize = Vector2I.One;
    [Export] private string[] InventoryDefaultItems;


    private const string PLAYER_INVENTORY_FILE_NAME = "player_inventory.json";

    private ItemContainer playerContainer;

    public override void _Ready()
    {
        playerContainer = new();
        if (!LoadInventory())
        {
            Print.Debug("Loading default inventory");
            playerContainer.ResizeContainer(InventoryDefaultSize.ToInventoryPosition(), out _);
            if (InventoryDefaultItems is not null)
            {
                foreach (var ItemKey in InventoryDefaultItems)
                {
                    AcquireItem(ItemKey, 1);
                }
            }
        }
        Events.Data.SerializeAll += SaveInventory;
        Events.Inventory.GivePlayerItem += AcquireItem;
        Events.Inventory.ConsumePlayerItem += ConsumeItem;
    }

    public override void _ExitTree()
    {
        Events.Data.SerializeAll -= SaveInventory;
        Events.Inventory.GivePlayerItem -= AcquireItem;
        Events.Inventory.ConsumePlayerItem -= ConsumeItem;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("open_inventory"))
        {
            Events.GUI.TriggerRequestInventory(playerContainer, null);
            this.HandleInput();
        }
    }

    private void AcquireItem(string itemKey, int count)
    {
        var item = ItemRegistry.GetItem(itemKey);
        var stack = new ItemStack(item, count);
        Print.Debug($"Trying to add to player inventory: {stack}");
        playerContainer.AddStack(ref stack);
    }

    private void ConsumeItem(string itemKey, int count)
    {
        var item = ItemRegistry.GetItem(itemKey);
        var stack = new ItemStack(item, count);
        Print.Debug($"Trying to consume from player inventory: {stack}");
        playerContainer.Consume(stack);
    }

    private void SaveInventory()
    {
        var data = playerContainer.GetSaveData();
        var stringData = Json.Stringify(data, "\t");
        Data.CurrentSaveSlot.SaveText(stringData, PLAYER_INVENTORY_FILE_NAME);
    }

    private bool LoadInventory()
    {
        var stringData = Data.CurrentSaveSlot.LoadText(PLAYER_INVENTORY_FILE_NAME, false);
        if (stringData is null) return false;
        var data = Json.ParseString(stringData).AsGodotDictionary();
        if (data is null) return false;

        playerContainer.LoadFromSaveData(data);
        return true;
    }

}
