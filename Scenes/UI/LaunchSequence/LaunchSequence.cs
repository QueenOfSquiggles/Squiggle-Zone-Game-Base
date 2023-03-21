using Godot;
using queen.data;
using queen.error;
using queen.extension;
using System;

public partial class LaunchSequence : Control
{
	[Export] private bool testing = false;
	[Export(PropertyHint.File, "*.tscn")] private string main_menu_scene;
	[Export(PropertyHint.Range, "0,1,0.01")] private float chance_do_anyway = 0.25f;
	[Export] private NodePath path_anim;
	private AnimationPlayer anim;

	public override void _Ready()
	{
		#if DEBUG
			// For debug builds. Never run it, excepting first time launches.
			// Demo overrides
			Print.Debug("Detected Debug Build");
			chance_do_anyway = -2.0f;
		#else
			if (OS.HasFeature("demo"))
			{
				// For the demo run animation every time
				Print.Debug("Detected Demo Build");
				chance_do_anyway = 2.0f;
			} else {
				Print.Debug("Detected Release Build");
			}
		#endif

		this.GetNode(path_anim, out anim);
		var ran = new Random();
		if (!Stats.Instance.FirstTimeLaunch && (ran.NextSingle() > chance_do_anyway))
		{
			#if DEBUG

			if(!testing)
			{
				EndLaunchSequence();
				return;
			} else {
				// technically doesn't have to be fixed. But for professionalism I want it to be
				Print.Warn("Launch Sequence is currently set to testing!!! Clear this before release!!!");
			}

			#else

			EndLaunchSequence();
			return;

			#endif
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
