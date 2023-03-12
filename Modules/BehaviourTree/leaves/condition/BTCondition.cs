namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTCondition : Node, IBTNode
{
    public virtual BTResult TickNode(BehaviourTree root) => SUCCESS;

}
