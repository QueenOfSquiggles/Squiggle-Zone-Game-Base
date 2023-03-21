namespace queen.items;

using System;
using System.Collections.Generic;
using Godot;
using queen.error;
using queen.extension;

public partial class ItemRegistry : Node
{
    private const string ITEMS_RESOURCES_PATH = "res://Game/Resource/Items";
    public const string ITEM_ERROR_KEY = "item.error";

    private static ItemRegistry _instance = null;

    private static Dictionary<string, Item> Items = new();
    private static Item ItemError = null;

    public override void _Ready()
    {
        if (!this.EnsureSingleton(ref _instance)) return;
        LoadItems();
    }

    /// <summary>
    /// Check whether this singleton has been properly loaded yet. Ideally it gets initialized on launch, but loads resources in a later step
    /// </summary>
    /// <returns>True is this singleton is ready to be interacted with.</returns>
    public static bool IsValid() => _instance != null;

    public static void LoadItems()
    {
        using var dir = DirAccess.Open(ITEMS_RESOURCES_PATH);
        if (dir is null)
        {
            Print.Error($"Failed to load items! Make sure the folder path is present in the current project! Else disable items if we don't want items in this game!!!\n\tExpected Item Path: '{ITEMS_RESOURCES_PATH}'");
            return;
        }
        dir.IncludeHidden = false;
        dir.IncludeNavigational = false;
        var itemDirs = dir.GetDirectories();
        foreach (var itemDir in itemDirs)
            LoadItemsFrom(ITEMS_RESOURCES_PATH.PathJoin(itemDir));
    }

    private static void LoadItemsFrom(string directory)
    {
        using var dir = DirAccess.Open(directory);
        if (dir is null) return;
        dir.IncludeHidden = false;
        dir.IncludeNavigational = false;
        var files = dir.GetFiles();

        foreach (var f in files)
        {
            if (!IsResourceFile(f)) continue;

            var resource = ResourceLoader.Load<Item>(directory.PathJoin(f));
            if (resource is not Item item) continue;

            Items.Add(item.ItemKey, item);
            if (item.ItemKey == ITEM_ERROR_KEY) ItemError = item;
            Print.Debug($"Loaded Item {item.ItemKey} from group {directory.GetBaseDir()}");
        }
    }

    private static bool IsResourceFile(string file)
    {
        var lowerFile = file.ToLower();
        if (lowerFile.EndsWith("tres")) return true;
        if (lowerFile.EndsWith("res")) return true;
        return false;
    }

    public static Item GetItem(string ItemKey)
    {
        if (!IsValid())
        {
            Print.Error("Trying to get an item before items have been loaded! This is an issue");
            return null;
        }
        if (ItemKey is null) return ItemError;
        if (Items.ContainsKey(ItemKey)) return Items[ItemKey];
        return ItemError;
    }
}
