namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTSelectStar : Node, IBTNode
{
    public BTResult TickNode(BehaviourTree root)
    {
        bool is_failing = true;
        foreach (var node in GetChildren())
        {
            var bt = node as IBTNode;
            var result = bt.TickNode(root);
            if (result != FAILURE) is_failing = false;
            if (result == SUCCESS) return SUCCESS;
        }
        return is_failing? FAILURE : RUNNING;
    }
}
