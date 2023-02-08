#if TOOLS
namespace queen;

using Godot;
using queen.behaviour_tree;

[Tool]

public partial class HelperPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddAutoloadSingleton("Scenes", "res://addons/squiggle_zone/autoload/scenes.tscn");
		Script script = GD.Load<Script>("res://addons/squiggle_zone/modules/event/EventEmitter.cs");
		AddCustomType("EventEmitter", "Node", script, null);
		BehaviourTreePluginModule.RegisterTypes(this);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveAutoloadSingleton("Scenes");
		BehaviourTreePluginModule.UnregisterTypes(this);
	}
}

#endif
