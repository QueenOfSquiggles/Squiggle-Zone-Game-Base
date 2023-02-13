using Godot;
using queen.error;
using queen.events;
using queen.extension;
using System;

public partial class DefaultHUD : Control
{

    [Export] private NodePath path_label_subtitle;
    [Export] private NodePath path_label_alert;
    [Export] private NodePath path_anim_player;

    private Label lbl_subtitle;
    private Label lbl_alert;
    private AnimationPlayer anim;

    private const string ANIM_SHOW_SUBTITLE = "ShowSubtitle";
    private const string ANIM_SHOW_ALERT = "ShowAlert";

    public override void _Ready()
    {
        this.GetNode(path_label_subtitle, out lbl_subtitle);
        this.GetNode(path_label_alert, out lbl_alert);
        this.GetNode(path_anim_player, out anim);

        lbl_subtitle.Text = "";
        lbl_alert.Text = "";
        
        Events.GUI.RequestSubtitle += ShowSubtitle;
        Events.GUI.RequestAlert += ShowAlert;
    }

    public override void _ExitTree()
    {
        Events.GUI.RequestSubtitle -= ShowSubtitle;
        Events.GUI.RequestAlert -= ShowAlert;
    }

    public async void ShowSubtitle(string text)
    {
        if (text.Length <= 0)
        {
            anim.PlayBackwards(ANIM_SHOW_SUBTITLE);
            await anim.WaitForCurrentAnimEnd();
            lbl_subtitle.Text = text;
        } else {
            var is_hidden = lbl_subtitle.Text.Length <= 0;
            lbl_subtitle.Text = text;
            if (is_hidden) anim.Play(ANIM_SHOW_SUBTITLE);            
        }
    }

    public async void ShowAlert(string text)
    {
        if (text.Length <= 0)
        {
            anim.PlayBackwards(ANIM_SHOW_ALERT);
            await anim.WaitForCurrentAnimEnd();
            lbl_alert.Text = text;
        } else {
            var is_hidden = lbl_alert.Text.Length <= 0;
            lbl_alert.Text = text;
            if (is_hidden) anim.Play(ANIM_SHOW_ALERT);            
        }

    }

}
