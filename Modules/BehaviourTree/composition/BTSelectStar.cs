namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTSelect : Node, IBTNode
{
    private IBTNode current = null;

    public BTResult TickNode(BehaviourTree root)
    {
        if (current != null) return TickSpecial(root);
        return TickStandard(root);
    }

    private BTResult TickSpecial(BehaviourTree root)
    {
        var result = current.TickNode(root);
        if (result == SUCCESS) return SUCCESS;

        var node = current as Node;
        current = null;
        return TickStandard(root, node.GetIndex(), result == FAILURE);
    }
    private BTResult TickStandard(BehaviourTree root, int start_index = 0, bool is_failing_start = true)
    {
        bool is_failing = is_failing_start;
        for (int i = start_index; i < GetChildCount(); i++)
        {
            var bt = GetChild(i) as IBTNode;
            var result = bt.TickNode(root);
            if (result != FAILURE) is_failing = false;
            if (result == SUCCESS) return SUCCESS;
        }
        return is_failing? FAILURE : RUNNING;
    }
}
