namespace queen.behaviour_tree;

using Godot;
using queen.extension;
using System.Collections.Generic;

public partial class BehaviourTree : Node
{

    [Export] private NodePath actor_path;

    public Dictionary<string, object> Blackboard => _blackboard;
    private readonly Dictionary<string, object> _blackboard = new();

    public Node Actor => _actor;
    private Node _actor;

    public override void _Ready()
    {
        this.GetNode(actor_path, out _actor);
    }

    public override void _PhysicsProcess(double delta)
    {
        var start = GetChild(0) as IBTNode;
        start.TickNode(this);
    }

}
