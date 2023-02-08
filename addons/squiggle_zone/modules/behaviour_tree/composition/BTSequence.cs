namespace queen.behaviour_tree;

using Godot;
using System;

public partial class BTSequence : Node, IBTNode
{
    public BTResult TickNode(BehaviourTree root)
    {
        foreach (var node in GetChildren())
        {
            var bt = node as IBTNode;
            var result = bt.TickNode(root);
            if (result == BTResult.FAILURE) return BTResult.FAILURE;
        }
        return BTResult.SUCCESS;
    }
}
