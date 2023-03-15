using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(ItemDB))]
public partial class ItemDB : Resource
{

    [Export] private Resource[] ItemRegistry;

    public Item GetItemBy(string name)
    {
        foreach (var i in ItemRegistry)
        {
            if (i is Item item && item.ItemName == name) return item;
        }
        return null;
    }

}