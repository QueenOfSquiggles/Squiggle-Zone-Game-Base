namespace queen.behaviour_tree;

using Godot;
using System;

using static BTResult;

public partial class BTSequenceStar : Node, IBTNode
{ // I love bitbrain's beehave design, so I'm kindof stealing this concept from there.
    private IBTNode current = null;
    public BTResult TickNode(BehaviourTree root)
    {
        if (current != null) return TickSpecial(root);
        return TickStandard(root);
    }

    private BTResult TickSpecial(BehaviourTree root)
    {
        var result = current.TickNode(root);
        if (result == FAILURE)
        {
            current = null;
            return FAILURE;
        }
        if (result == SUCCESS)
        {
            current = null;
            var node = current as Node;
            return TickStandard(root, node.GetIndex());
        }

        return RUNNING;
    }
    private BTResult TickStandard(BehaviourTree root, int start_index = 0)
    {
        for (int i = start_index; i < GetChildCount(); i++)
        {
            var bt = GetChild(i) as IBTNode;
            var result = bt.TickNode(root);
            if (result == RUNNING)
            {
                current = bt;
                return RUNNING;
            } 
            if (result == FAILURE) return FAILURE;
        }
        return SUCCESS;

    }
}
