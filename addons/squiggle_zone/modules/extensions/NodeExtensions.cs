using Godot;
using queen.error;

namespace queen.extension;

public static class NodeExtensions
{

    public static void GetNode<T>(this Node node, NodePath path, out T result) where T : class
    {
        result = node.GetNode(path) as T;
        if (result == null)
        {
            Print.Error($"Node initialization failure: {node.Name} failed to acquire node from path {path}. Returning null");
        }
    }

    public static void HandleInput(this Node node)
    {
        node.GetViewport().SetInputAsHandled();
    }

    public static bool EnsureSingleton<T>(this Node node, ref T instance) where T : Node
    {
        if (instance != null)
        {
            Print.Error($"Found duplicate of singleton!\n\tType={instance.GetType().FullName}\n\tTree Path={node.GetPath()}");
            node.QueueFree();
            return false;
        }
        instance = node as T;
        return true;
    }

}