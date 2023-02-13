using Godot;
using queen.error;
using queen.events;
using queen.extension;
using System;

public partial class DefaultHUD : Control
{

    [Export] private NodePath path_label_subtitle;
    [Export] private NodePath path_label_alert;

    [Export] private NodePath path_root_subtitle;
    [Export] private NodePath path_root_alert;

    private Label lbl_subtitle;
    private Label lbl_alert;
    private Control root_subtitle;
    private Control root_alert;

    private Color COLOUR_TRANSPARENT = Color.FromString("#FFFFFF00", Colors.White);
    private Color COLOUR_VISIBLE = Colors.White;
    public override void _Ready()
    {
        this.GetNode(path_label_subtitle, out lbl_subtitle);
        this.GetNode(path_label_alert, out lbl_alert);
        this.GetNode(path_root_subtitle, out root_subtitle);
        this.GetNode(path_root_alert, out root_alert);

        lbl_subtitle.Text = "";
        lbl_alert.Text = "";
        
        Events.GUI.RequestSubtitle += ShowSubtitle;
        Events.GUI.RequestAlert += ShowAlert;

        root_subtitle.Modulate = COLOUR_TRANSPARENT;
        root_alert.Modulate = COLOUR_TRANSPARENT;
    }

    public override void _ExitTree()
    {
        Events.GUI.RequestSubtitle -= ShowSubtitle;
        Events.GUI.RequestAlert -= ShowAlert;
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
        var colour = isVisible? COLOUR_VISIBLE : COLOUR_TRANSPARENT;
        tween.TweenProperty(control, "modulate", colour, 0.2f);
    }

}
