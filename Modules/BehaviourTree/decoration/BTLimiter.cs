namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTLimiter : Node, IBTNode
{

    [Export] private int limit = 5;
    private int execution_count = 0;
    public BTResult TickNode(BehaviourTree root)
    {
        if (execution_count > limit) return SUCCESS;
        execution_count++;
        return (GetChild(0) as IBTNode).TickNode(root);
    }
}
