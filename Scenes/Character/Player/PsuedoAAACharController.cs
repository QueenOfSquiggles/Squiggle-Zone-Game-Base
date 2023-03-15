using System;
using Godot;
using queen.data;
using queen.events;
using queen.extension;

/// <summary>
/// The goal of this controller is to emulate the kind of controller that AAA horror games would use. Specifically targeting Resident Evil 7 (Biohazard). Whether or not is succeeds is up for debate.
/// </summary>
public partial class PsuedoAAACharController : CharacterBody3D
{
    [ExportGroup("Movement Stats")]
    [Export] private float Speed = 2.0f;
    [Export] private float SprintSpeed = 5.0f;
    [Export] private float Acceleration = 0.3f;
    [Export] private float JumpVelocity = 4.5f;
    [Export] private float mouse_sensitivity;
    [Export] private float CrouchSpeedScale = 0.45f;

    [ExportGroup("Node Paths")]
    [Export] private NodePath PathVCam;
    [Export] private NodePath PathGroundMaterialPoll;
    [Export] private NodePath PathAnimationPlayer;
    [Export] private NodePath PathCanStandCheck;

    // References
    private VirtualCamera vcam;
    private GroundMaterialPoller groundMaterialPoller;
    private AnimationPlayer anim;
    private RayCast3D CanStandCheck;

    // Values
    private Vector2 camera_look_vector = new();
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    private float CurrentSpeed = 0.0f;
    private bool IsCrouching = false;

    public override void _Ready()
    {
        this.GetSafe(PathVCam, out vcam);
        this.GetSafe(PathGroundMaterialPoll, out groundMaterialPoller);
        this.GetSafe(PathAnimationPlayer, out anim);
        this.GetSafe(PathCanStandCheck, out CanStandCheck);

        Events.Gameplay.RequestPlayerAbleToMove += HandleEventPlayerCanMove;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        CamLookLogic(delta);

        Vector3 velocity = Velocity;
        CamMoveLogic(ref velocity, delta);
        if (!IsCrouching) JumpLogic(ref velocity, delta);

        Velocity = velocity;
        MoveAndSlide();
    }

    private void JumpLogic(ref Vector3 velocity, double delta)
    {
        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;
    }

    private void CamMoveLogic(ref Vector3 velocity, double delta)
    {

        var input_dir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        if (input_dir.LengthSquared() < 0.1f)
            input_dir = Input.GetVector("gamepad_move_left", "gamepad_move_right", "gamepad_move_forward", "gamepad_move_back");

        Vector3 direction = (Transform.Basis * new Vector3(input_dir.X, 0, input_dir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {

            var target_speed = IsCrouching ? Speed * CrouchSpeedScale : ((IsOnFloor() && Input.IsActionPressed("sprint")) ? SprintSpeed : Speed);
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, target_speed, Acceleration);
            velocity.X = direction.X * CurrentSpeed;
            velocity.Z = direction.Z * CurrentSpeed;
        }
        else
        {
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0, Acceleration);
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrentSpeed);
        }
    }

    private void CamLookLogic(double delta)
    {
        var look = (camera_look_vector.LengthSquared() > 0.1f) ? camera_look_vector : GetGamepadLookVector();
        look *= (float)delta;

        RotateY(look.X * mouse_sensitivity);

        var rot = vcam.Rotation;
        rot.X += look.Y * mouse_sensitivity;
        var cl = Mathf.DegToRad(89.0f);
        rot.X = Mathf.Clamp(rot.X, -cl, cl);
        vcam.Rotation = rot;

        camera_look_vector = Vector2.Zero;
    }

    private Vector2 GAMEPAD_VEC_FLIP = new(-1, 1);
    private Vector2 GetGamepadLookVector()
    {
        return Input.GetVector("gamepad_look_left", "gamepad_look_right", "gamepad_look_down", "gamepad_look_up")
        * Controls.Instance.ControllerLookSensitivity
        * GAMEPAD_VEC_FLIP;
    }


    private void HandleEventPlayerCanMove(bool can_move)
    {
        SetPhysicsProcess(can_move);
        // prevents random motion after returning
        Velocity = Vector3.Zero;
        camera_look_vector = Vector2.Zero;
    }
    public override void _UnhandledInput(InputEvent e)
    {
        bool handled = false;
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            handled = handled || InputMouseLook(e);
            handled = handled || InputInteract(e);
            handled = handled || InputCrouch(e);
            // TODO add in other controls here!

        }
        if (handled) GetViewport().SetInputAsHandled();
    }
    private bool InputMouseLook(InputEvent e)
    {
        if (e is not InputEventMouseMotion) return false;
        var mm = e as InputEventMouseMotion;
        camera_look_vector += mm.Relative * Controls.Instance.MouseLookSensivity * -1f;
        return true;
    }

    private bool InputInteract(InputEvent e)
    {
        return false; // TODO fixme
        // if (!e.IsActionPressed("interact")) return false;

        // raycast.ForceRaycastUpdate();
        // if (raycast.GetCollider() is Node collider && collider is IInteractable inter && inter.IsActive())
        // {
        //     inter.Interact();
        // }
        // return true;
    }

    private bool InputCrouch(InputEvent e)
    {
        if (!e.IsActionPressed("crouch")) return false;
        if (IsCrouching && CanStandCheck.IsColliding()) return false;
        IsCrouching = !IsCrouching;
        if (IsCrouching) anim.Play("Crouch");
        else anim.PlayBackwards("Crouch");

        return true;
    }

}
