namespace queen.behaviour_tree;

using Godot;

using static BTResult;
public partial class BTSucceeder : Node, IBTNode
{
    public BTResult TickNode(BehaviourTree root)
    {
        (GetChild(0) as IBTNode).TickNode(root);

        return SUCCESS;
    }
}
