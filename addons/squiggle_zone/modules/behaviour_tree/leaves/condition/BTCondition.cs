namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public abstract partial class BTCondition : Node, IBTNode
{
    public abstract BTResult TickNode(BehaviourTree root);
}
