using Godot;
using queen.data;
using queen.error;
using queen.extension;
using System;

public partial class LaunchSequence : Control
{
    [Export(PropertyHint.File, "*.tscn")] private string main_menu_scene;
    [Export(PropertyHint.Range, "0,1,0.01")] private float chance_do_anyway = 0.25f;
    [Export] private NodePath path_anim;
    private AnimationPlayer anim;

    public override void _Ready()
    {
        #if GODOT_DEMO
            // For the demo run animation every time
            Print.Info("Detected Demo Build");
            chance_do_animation_on_repeated_launch = 2.0f;
        #elif DEBUG
            // For debug builds. Never run it, excepting first time launches.
            // Demo overrides
            Print.Info("Detected Debug Build");
            chance_do_anyway = -2.0f;
        #else
            Print.Info("Detected Release Build");
        #endif

        this.GetNode(path_anim, out anim);
        var ran = new Random();
        if (!Stats.Instance.FirstTimeLaunch && ran.NextSingle() > chance_do_anyway) 
        {
            EndLaunchSequence();
            return;
        }

        Stats.Instance.FirstTimeLaunch = false;
        Stats.SaveSettings();
        anim.Play("OpeningAnimation");
    }

    public void EndLaunchSequence()
    {
        Scenes.LoadSceneAsync(main_menu_scene);
    }


}
