namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public abstract partial class BTAction : Node, IBTNode
{
    public abstract BTResult TickNode(BehaviourTree root);
}
