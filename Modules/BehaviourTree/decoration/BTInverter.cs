namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTInverter : Node, IBTNode
{
    public BTResult TickNode(BehaviourTree root)
    {
        var result = (GetChild(0) as IBTNode).TickNode(root);
        if (result == RUNNING) return RUNNING;
        return result == SUCCESS? FAILURE : SUCCESS;
    }
}
