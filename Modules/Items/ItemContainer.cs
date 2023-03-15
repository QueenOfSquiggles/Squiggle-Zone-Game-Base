using System;
using System.Collections.Generic;
using Godot;
using Godot.NativeInterop;
using MonoCustomResourceRegistry;
using queen.data;

[RegisteredType(nameof(ItemContainer))]
public partial class ItemContainer : Resource
{
    [Export] public string ItemContainerName = "";
    [Export] public Vector2I ContainerSize = new(1, 1);
    [Export] private ItemDB DB;

    private Dictionary<Vector2I, ItemStack> ItemGrid = new();
    private const string CONTAINERS_SUBDIR = "container";

    public bool AddItem(Item item, int count = 1)
    {
        int remaining = count;
        foreach (var stack in ItemGrid.Values)
        {
            if (stack.item == item && stack.count < item.MaxStackSize)
            {
                int allowance = item.MaxStackSize - stack.count;
                if (allowance >= remaining)
                {
                    stack.count += remaining;
                    return true;
                }
                stack.count += allowance;
                remaining -= allowance;
                // TODO double check my math is good on this.
                if (remaining <= 0) return true;
            }
        }

        var position = FindValidPosition(item.ItemInventorySize);
        if (position is null) return false;
        var new_stack = new ItemStack()
        {
            item = item,
            count = count
        };
        ItemGrid[position.Value] = new_stack;
        return true;
    }

    public bool ConsumeItem(Item item, int count = 1)
    {
        int remaining = count;
        foreach (var stack in ItemGrid.Values)
        {
            if (stack.item == item)
            {
                if (stack.count > remaining)
                {
                    stack.count -= remaining;
                    return true;
                }
                remaining -= stack.count;

                // TODO double check my math is good on this.
                if (remaining <= 0) return true;
            }
        }
        return false;
    }

    public ItemStack FindItem(Item item)
    {
        foreach (var stack in ItemGrid.Values)
        {
            if (stack.item == item && stack.count > 0) return stack;
        }
        return null;
    }


    private Vector2I? FindValidPosition(Vector2I size)
    {
        // TODO this function does not currently consider rotation as an option. If all objects are square that works, but maybe I'll want organizational options in the future? TBD for now

        var to_add = new Rect2I(new Vector2I(0, 0), size);
        int indices = ContainerSize.X * ContainerSize.Y;
        for (int i = 0; i < indices; i++)
        {

            to_add.Position = new(i % ContainerSize.X, i / ContainerSize.X);

            bool is_blocked = false;
            foreach (var pos in ItemGrid.Keys)
            {
                is_blocked |= GetFor(pos) is Rect2I rect && rect.Intersects(to_add);
                if (is_blocked) break;
            }
            if (!is_blocked) return to_add.Position;

        }
        return null;
    }

    private Rect2I? GetFor(Vector2I position)
    {
        if (!ItemGrid.ContainsKey(position)) return null;
        if (ItemGrid[position] is not ItemStack stack || stack.item is null) return null;

        var size = stack.item.ItemInventorySize;
        return new Rect2I(position, size);
    }

    public void Serialize()
    {
        var data = new Godot.Collections.Dictionary();
        foreach (var entry in ItemGrid)
        {
            var key = $"{entry.Key.X},{entry.Key.Y}";
            var dict = new Godot.Collections.Dictionary();
            dict.Add("name", entry.Value.item.ItemName);
            dict.Add("count", entry.Value.count);
            if (entry.Value.item.HasCustomData && entry.Value.item.GetCustomData() is Godot.Collections.Dictionary custom_data)
            {
                dict.Add("custom_data", custom_data);
            }
            data.Add(key, dict); // nested objects
        }
        var text_data = Json.Stringify(data, "\t");
        Data.CurrentSaveSlot.SaveText(text_data, GetSavePath());
    }

    public void Deserialize()
    {
        var text_data = Data.CurrentSaveSlot.LoadText(GetSavePath());
        if (text_data is null) return;
        var dict = Json.ParseString(text_data).AsGodotDictionary();
        if (dict is null) return;
        foreach (var entry in dict)
        {
            var pos = ParsePositionString(entry.Key.AsString());
            if (pos is null) continue;
            var data = entry.Value.AsGodotDictionary();
            if (data is null) continue;

            var stack = new ItemStack();
            var item = DB.GetItemBy(data["name"].AsString());
            if (item is null) continue;
            stack.item = item;
            stack.count = data["count"].AsInt32();

            if (data.ContainsKey("custom_data"))
            {
                var custom_data = data["custom_data"].AsGodotDictionary();
                if (custom_data is not null) stack.item.LoadCustomData(custom_data);
            }

        }
    }

    private Vector2I? ParsePositionString(string data)
    {
        // "(000,000)"
        var parts = data.Split(",");
        if (parts.Length < 2) return null;
        int x, y;
        if (!int.TryParse(parts[0], out x) || !int.TryParse(parts[1], out y)) return null;
        return new Vector2I(x, y);
    }

    private string GetSavePath()
    {
        return $"{CONTAINERS_SUBDIR}/{ItemContainerName.ToCamelCase()}.json";
    }

}
