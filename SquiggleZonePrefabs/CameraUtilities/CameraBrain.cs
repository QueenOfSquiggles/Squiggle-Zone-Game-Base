using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CameraBrain : Camera3D
{
    [Export(PropertyHint.Enum, "Process,Physics")] private int update_mode = 0;
    private const int UPDATE_PROCESS = 0;
    private const int UPDATE_PHYSICS = 0;

    // treating this as a stack, but using list to let me remove elements anywhere
    private readonly List<VirtualCamera> vcam_stack = new();
    private VirtualCamera current_target;

    public Vector3 Offset = new();

//
// API
//

    public void PushCamera(VirtualCamera vcam)
    {
        vcam_stack.Insert(0, vcam);
    }

    public void PopCamera(VirtualCamera vcam)
    {
        vcam_stack.Remove(vcam);
    }

    public bool HasCamera(VirtualCamera vcam)
    {
        return vcam_stack.Contains(vcam);
    }

    //
    //  Background Systems
    //
    public override void _Ready()
    {
        TopLevel = true;
    }
    public override void _Process(double delta)
    {
        if (update_mode != UPDATE_PROCESS) return;
        UpdateCamera((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (update_mode != UPDATE_PHYSICS) return;
        UpdateCamera((float)delta);
    }

    private void UpdateCamera(float delta)
    {
        if (vcam_stack.Count <= 0) return;
        var target = vcam_stack[0];

        GlobalTransform = target.LerpCamera ?
            GlobalTransform.InterpolateWith(target.GlobalTransform, target.LerpFactor * delta) :
            target.GlobalTransform;

        GlobalPosition += Offset;

        // GlobalPosition = target.LerpCamera ?
        //     GlobalPosition.Lerp(target.GlobalPosition, target.LerpFactorPosition * delta) + Offset :
        //     target.GlobalPosition + Offset;

        // GlobalRotation = target.LerpCamera ?
        //     GlobalRotation.Lerp(target.GlobalRotation, target.LerpFactorRotation * delta) :
        //     target.GlobalRotation;
    }


}
