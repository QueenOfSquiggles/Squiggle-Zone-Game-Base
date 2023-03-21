using Godot.Collections;

namespace queen.items;

public class ItemStack
{

    public Item StackItem;
    public int StackCount = 1;
    public bool IsStackable => StackItem is not null && StackItem.IsStackable;
    public int MaxStackSize => StackItem is not null ? StackItem.MaxStackSize : 1;
    public bool IsFull => StackCount >= MaxStackSize;
    public bool IsEmpty => StackCount <= 0;
    public Dictionary CustomItemData = new();

    public ItemStack()
    {
        StackItem = ItemRegistry.GetItem("item.error");
        StackCount = 0;
    }
    public ItemStack(Item item, int count = 1)
    {
        StackItem = item;
        StackCount = count;
    }

    public ItemStack Copy() => (ItemStack)MemberwiseClone();

    public Dictionary SaveStack()
    {
        if (StackItem is null) return new Dictionary()
        {
            { "item", "null" },
            { "count", StackCount },
            { "custom_data", CustomItemData}
        };
        CustomItemData = StackItem.SaveInstanceData(this);
        var data = new Dictionary
        {
            { "item", StackItem.ItemKey },
            { "count", StackCount },
            { "custom_data", CustomItemData}
        };
        return data;
    }

    public void LoadStack(Dictionary data)
    {
        var itemKey = data["item"].AsString();
        var itemCount = data["count"].AsInt32();
        var itemData = data["custom_data"].AsGodotDictionary();
        StackItem = ItemRegistry.GetItem(itemKey);
        StackCount = itemCount;
        CustomItemData = itemData;
        if (StackItem == null) return;
    }

    public bool SameItem(ItemStack other)
    {
        return StackItem == other.StackItem;
    }

    public bool IsValidStack()
    {
        if (IsEmpty) return false;
        if (StackItem == null) return false;
        return true;
    }

    public override string ToString()
    {
        return $"Stack({StackItem} X {StackCount})";
    }

}
