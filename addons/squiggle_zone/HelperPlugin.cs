#if TOOLS
namespace queen;

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
		Script script = GD.Load<Script>("res://addons/squiggle_zone/modules/event/EventEmitter.cs");
		AddCustomType("EventEmitter", "Node", script, null);
		BehaviourTreePluginModule.RegisterTypes(this);
		AddToolMenuItem(TOOL_MENU_OPEN_CONFIG, new Callable(this, nameof(ToolEditConfiguration)));
		AddToolMenuItem(TOOL_MENU_RELEASE, new Callable(this, nameof(ToolReleaseBuilds)));
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveAutoloadSingleton("Scenes");
		BehaviourTreePluginModule.UnregisterTypes(this);
		RemoveToolMenuItem(TOOL_MENU_OPEN_CONFIG);
		RemoveToolMenuItem(TOOL_MENU_RELEASE);
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
		OS.Execute("/usr/bin/konsole", new string[]{ "--workdir", script.GetBaseDir(), "-e", PY_CMD, script });
	}

}

#endif
