namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTAction : Node, IBTNode
{
    public virtual BTResult TickNode(BehaviourTree root) => SUCCESS;
}
