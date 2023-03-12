#if TOOLS
namespace queen;

using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using queen.behaviour_tree;
using queen.error;

[Tool]

public partial class HelperPlugin : EditorPlugin
{
	private const string TOOL_MENU_OPEN_CONFIG = "Open Release Configuration";
	private const string TOOL_MENU_CLEAN = "Clean exports folder";
	private const string TOOL_MENU_RELEASE = "Release project to Itch.io";
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddAutoloadSingleton("Scenes", "res://addons/squiggle_zone/autoload/scenes.tscn");
		AddAutoloadSingleton("BGM", "res://addons/squiggle_zone/autoload/bgm.tscn");

		Script script = GD.Load<Script>("res://addons/squiggle_zone/modules/event/EventEmitter.cs");
		AddCustomType("EventEmitter", "Node", script, null);
		Script bgm_script = GD.Load<Script>("res://addons/squiggle_zone/modules/SceneBGMLoader.cs");
		AddCustomType("SceneBGMLoader", "Node", bgm_script, null);




		BehaviourTreePluginModule.RegisterTypes(this);
		AddToolMenuItem(TOOL_MENU_OPEN_CONFIG, new Callable(this, nameof(ToolEditConfiguration)));
		AddToolMenuItem(TOOL_MENU_RELEASE, new Callable(this, nameof(ToolReleaseBuilds)));
		AddToolMenuItem(TOOL_MENU_CLEAN, new Callable(this, nameof(ToolCleanBuilds)));
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveAutoloadSingleton("Scenes");
		RemoveAutoloadSingleton("BGM");
        RemoveCustomType("EventEmitter");
        RemoveCustomType("SceneBGMLoader");
        BehaviourTreePluginModule.UnregisterTypes(this);
		RemoveToolMenuItem(TOOL_MENU_OPEN_CONFIG);
		RemoveToolMenuItem(TOOL_MENU_RELEASE);
		RemoveToolMenuItem(TOOL_MENU_CLEAN);
	}

	private const string EXPORT_DIR = "res://export";
	private const string PY_RELEASE = "res://release_builds.py";
	private const string PY_CLEAN = "res://";
	private const string PY_CMD = "python3";
	private const string CONFIG_RELEASE = "res://release_config.cfg";

	public void ToolEditConfiguration() => OS.ShellOpen(ProjectSettings.GlobalizePath(CONFIG_RELEASE));

	public void ToolReleaseBuilds()
	{
		var script = ProjectSettings.GlobalizePath(PY_RELEASE);
		var output = new Array();
		RunTerminalCommand(script.GetBaseDir(), script, System.Array.Empty<string>());
		PrintOutput(output);
	}

	public void ToolCleanBuilds()
	{
		var script = ProjectSettings.GlobalizePath(PY_RELEASE);
		RunTerminalCommand(script.GetBaseDir(), script, new string[]{"./export","--clean"});
	}

	private void RunTerminalCommand(string dir, string py_script, string[] args)
	{
		var output = new Array();
		Print.Info($"Running terminal command:\n\t{py_script.GetFile()}: {args}");
		string[] cmd_buffer = 
			(args.Length <= 0)?
			new string[]{ "--hold", "--workdir", dir, "-e", PY_CMD, py_script}:
			new string[]{ "--hold", "--workdir", dir, "-e", PY_CMD, py_script, args[0], args[1]};
			// I don't like hard coding it, but this really doesn't have to scale so it's no big deal

		OS.Execute("/usr/bin/konsole", cmd_buffer, output, true);
		PrintOutput(output);
	}

	private void PrintOutput(Array lines)
	{
		Print.MsgOut(new Msg("--- Command Output ---").AlignCenter().Color("pink"));
		foreach (var l in lines)
		{
			Print.MsgOut(new Msg($":: {l.AsString()}").Monospaced().Color("grey"));
		}
	}

}

#endif
