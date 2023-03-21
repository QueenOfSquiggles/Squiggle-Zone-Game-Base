using System;
using System.Threading.Tasks;
using Godot;
using queen.error;
using queen.events;
using queen.extension;

public partial class DefaultHUD : Control
{

    [Export] private NodePath path_label_subtitle;
    [Export] private NodePath path_label_alert;

    [Export] private NodePath path_root_subtitle;
    [Export] private NodePath path_root_alert;
    [Export] private NodePath path_reticle;
    [Export] private NodePath path_interaction_prompt;
    [Export] private NodePath path_inventory_display;


    private Label lbl_subtitle;
    private Label lbl_alert;
    private Control root_subtitle;
    private Control root_alert;
    private TextureRect reticle;
    private Label interaction_prompt;
    private Control inventory_display;

    private Color COLOUR_TRANSPARENT = Color.FromString("#FFFFFF00", Colors.White);
    private Color COLOUR_VISIBLE = Colors.White;
    private Tween prompt_tween;
    public override void _Ready()
    {
        this.GetNode(path_label_subtitle, out lbl_subtitle);
        this.GetNode(path_label_alert, out lbl_alert);
        this.GetNode(path_root_subtitle, out root_subtitle);
        this.GetNode(path_root_alert, out root_alert);
        this.GetNode(path_reticle, out reticle);
        this.GetNode(path_interaction_prompt, out interaction_prompt);
        this.GetNode(path_inventory_display, out inventory_display);

        inventory_display.Visible = false;
        lbl_subtitle.Text = "";
        lbl_alert.Text = "";


        root_subtitle.Modulate = COLOUR_TRANSPARENT;
        root_alert.Modulate = COLOUR_TRANSPARENT;

        reticle.Scale = Vector2.Zero;
        interaction_prompt.Text = "";


        Events.GUI.RequestSubtitle += ShowSubtitle;
        Events.GUI.RequestAlert += ShowAlert;
        Events.GUI.MarkAbleToInteract += OnCanInteract;
        Events.GUI.MarkUnableToInteract += OnCannotInteract;
        Events.GUI.RequestInventory += OnDisplayInventory;
    }

    public override void _ExitTree()
    {
        Events.GUI.RequestSubtitle -= ShowSubtitle;
        Events.GUI.RequestAlert -= ShowAlert;
        Events.GUI.MarkAbleToInteract -= OnCanInteract;
        Events.GUI.MarkUnableToInteract -= OnCannotInteract;
        Events.GUI.RequestInventory -= OnDisplayInventory;
    }

    public void ShowSubtitle(string text)
    {
        lbl_subtitle.Text = text;
        HandleAnimation(root_subtitle, text.Length > 0);
    }

    public void ShowAlert(string text)
    {
        lbl_alert.Text = text;
        HandleAnimation(root_alert, text.Length > 0);
    }

    private void HandleAnimation(Control control, bool isVisible)
    {
        var tween = GetTree().CreateTween();
        var colour = isVisible ? COLOUR_VISIBLE : COLOUR_TRANSPARENT;
        tween.TweenProperty(control, "modulate", colour, 0.2f);
    }

    private void OnCanInteract(string text)
    {
        prompt_tween?.Kill();
        prompt_tween = GetTree().CreateTween();

        interaction_prompt.VisibleRatio = 0.0f;
        interaction_prompt.Text = text;

        prompt_tween.TweenProperty(reticle, "scale", Vector2.One, 0.3f);
        prompt_tween.TweenProperty(interaction_prompt, "visible_ratio", 1.0f, 0.3f);
    }

    private void OnCannotInteract()
    {
        prompt_tween?.Kill();
        prompt_tween = GetTree().CreateTween();

        prompt_tween.TweenProperty(reticle, "scale", Vector2.Zero, 0.3f);
        prompt_tween.TweenProperty(interaction_prompt, "visible_ratio", 0.0f, 0.1f);
    }

    private void OnDisplayInventory(bool IsVisible)
    {
        inventory_display.Visible = IsVisible;
        Input.MouseMode = IsVisible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        Input.WarpMouse(inventory_display.Size / 2);
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey kp && kp.Keycode == Key.F1 && kp.IsPressed())
        {
            Visible = !Visible; // toggle visibility of HUD for cinematics or other useful things
        }
        if (e.IsActionPressed("open_inventory") && inventory_display.Visible)
            Events.GUI.TriggerRequestInventory(false);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("open_inventory"))
        {
            if (inventory_display.Visible) Events.GUI.TriggerRequestInventory(false);
            this.HandleInput();
        }
    }

}
