using Godot;
using queen.error;
using System;

public partial class VirtualCamera : Marker3D
{
    [Export] private bool PushCamOnReady = false;
    [Export] private bool AllowVCamChildren = false;
    
    [ExportGroup("Camera Lerping")]
    [Export] public bool LerpCamera = true;
    [Export] private float LerpDuration = 0.1f;

    public float LerpFactor {
        get {
            return 1.0f / LerpDuration;
        }
    }


    public override void _Ready()
    {
        if (PushCamOnReady) PushVCam();
        if (GetChildCount() > 0 && !AllowVCamChildren)
        {
            Print.Warn("Removing VirtualCamera child nodes. These should be removed for release!");
            while(GetChildCount() > 0)
            {
                var child = GetChild(0);
                child.QueueFree();
                RemoveChild(child);
            }
        }
    }

    public void PushVCam() => GetBrain()?.PushCamera(this);

    public void PopVCam() => GetBrain()?.PopCamera(this);

    private CameraBrain GetBrain()
    {
        var brain = GetTree().GetFirstNodeInGroup("cam_brain") as CameraBrain;
        //Debugging.Assert(brain != null, "VirtualCamera failed to find CameraBrain in scene. Possibly it is missing??");
        return brain;
    }

    public override void _ExitTree()
    {
        var brain = GetBrain();
        if (brain == null) return; // Brain has already been cleared
        if (brain.HasCamera(this)) PopVCam();
    }


}
