using Godot;
using interaction;
using queen.error;
using queen.events;
using queen.extension;
using queen.items;

public partial class WorldItemInstance : StaticBody3D, IInteractable
{

    [Export] private Resource ItemResource;
    [Export] private int Quantity = 1;

    [ExportGroup("Paths")]
    [Export] private NodePath PathMeshInstance;
    [Export] private NodePath PathCollisionShape;

    private MeshInstance3D mesh;
    private CollisionShape3D collider;
    private Item item;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.GetSafe(PathMeshInstance, out mesh);
        this.GetSafe(PathCollisionShape, out collider);
        item = ItemResource as Item;
        if (item is null)
        {
            Print.Warn($"Item was found null. Failed to create world item instance");
            return;
        }

        mesh.Mesh = item.ItemMesh;
        if (mesh.Mesh is not null)
            collider.Shape = mesh.Mesh.CreateConvexShape(true, true);
    }


    public string GetActiveName()
    {
        return item is not null ? item.ItemKey : "null item";
    }

    public bool Interact()
    {
        if (item is null) return false;

        Events.Inventory.TriggerGivePlayerItem(item.ItemKey, Quantity);
        QueueFree();
        return true;
    }

    public bool IsActive()
    {
        return true;
    }


}