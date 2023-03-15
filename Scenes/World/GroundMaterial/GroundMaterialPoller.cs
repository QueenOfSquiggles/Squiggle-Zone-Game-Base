using Godot;
using queen.extension;
using System;

public partial class GroundMaterialPoller : RayCast3D
{

    [Signal] public delegate void OnNewMaterialFoundEventHandler(Material ground_material);
    [Export] private string GroundMaterialGroup = "HasGroundMaterial";
    public GroundMaterial Material { get; private set; } = null;

    public override void _PhysicsProcess(double _delta)
    {
        if (!IsColliding()) return;
        if (GetCollider() is not Node3D n3d) return;
        if (!n3d.IsInGroup(GroundMaterialGroup)) return;
        if (n3d.GetComponent<GroundMaterial>() is not GroundMaterial gm) return;

        if (Material != gm)
        {
            Material = gm;
            EmitSignal(nameof(OnNewMaterialFoundEventHandler), Material);
        }
    }
}
