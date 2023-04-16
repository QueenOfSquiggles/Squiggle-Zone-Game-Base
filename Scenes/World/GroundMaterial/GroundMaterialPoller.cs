using System;
using System.Threading.Tasks;
using Godot;
using queen.error;
using queen.extension;

public partial class GroundMaterialPoller : RayCast3D
{

    [Signal] public delegate void OnNewMaterialFoundEventHandler(GroundMaterial ground_material);
    [Export] private string GroundMaterialGroup = "HasGroundMaterial";
    public GroundMaterial? Material { get; private set; } = null;

    public override void _Ready() => DelayedForceUpdate(50);

    private async void DelayedForceUpdate(int delay_millis)
    {
        await Task.Delay(delay_millis);
        ForceRaycastUpdate();
    }

    public override void _PhysicsProcess(double _delta)
    {
        if (!IsColliding()) return;
        if (GetCollider() is not Node3D n3d) return;
        if (!n3d.IsInGroup(GroundMaterialGroup)) return;
        if (n3d.GetComponent<GroundMaterial>() is not GroundMaterial gm) return;

        if (Material is null || Material != gm)
        {
            Material = gm;
            Print.Info("Found ground material");
            EmitSignal(nameof(OnNewMaterialFound), Material);
        }
    }
}
