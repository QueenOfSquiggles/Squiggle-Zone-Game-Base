namespace queen.behaviour_tree;

public enum BTResult
{
    SUCCESS, FAILURE, RUNNING
}

public interface IBTNode
{
    public BTResult TickNode(BehaviourTree root);
}