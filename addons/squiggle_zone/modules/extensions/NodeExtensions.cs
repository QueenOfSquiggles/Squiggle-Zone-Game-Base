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

            #if DEBUG

            // LMAO I can get real dumb. This will make it SUPER obvious if I forget to set a node path. OS.Alert stops the main thread until the alert is cleared, so I am forced to read what happened and why.

            Input.MouseMode = Input.MouseModeEnum.Visible;
            string msg = $"Failed to acquire node for {node.Name}.\n({node.GetPath()})\n\nTarget Node:\n\t[{path}]\n\n";
            
            if (path == null || path == "") msg += "Looks like you forgot to assign the node path";
            else msg += "Looks like a type mis-match. Check the error log to be sure!";

            OS.Alert(msg, "GetNode failure!");

            #endif
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