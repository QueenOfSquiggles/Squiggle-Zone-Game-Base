using Godot;
using queen.error;

namespace queen.extension;

public static class NodeExtensions
{

    /// <summary>
    /// An effective override of "GetNode" with built-in null checking and a debug environment safeguard.
    /// </summary>
    /// <typeparam name="T">The type of the node to acquire</typeparam>
    /// <param name="node">The node we are searching from</param>
    /// <param name="path">The path (absolute or relative to node) to the node we want. Can be a string </param>
    /// <param name="result">Outputs the node if found, else null</param>
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
    /// <summary>
    /// A redirect to reduce confusion with the built-in function 'GetNode'
    /// </summary>
    /// <seealso cref="GetNode"/>
    public static void GetSafe<T>(this Node node, NodePath path, out T result) where T : class
    {
        node.GetNode(path, out result);
    }


    /// <summary>
    /// Utility to set input as handled. The proper way to do this requires checking into the Node's viewport, which is tedious at best.
    /// </summary>
    /// <param name="node"></param>
    public static void HandleInput(this Node node)
    {
        node?.GetViewport()?.SetInputAsHandled();
    }

    /// <summary>
    /// Used internally to ensure an autoloaded node is also a singleton on the C# side of things.
    /// </summary>
    /// <typeparam name="T">generally inferred from inputs. The type we are trying to make singleton</typeparam>
    /// <param name="node">The node to perform on</param>
    /// <param name="instance">A reference to the singleton's instance field. This is assigned to the node parameter if this is the first instance. Ideally this field will be static</param>
    /// <returns>True if this is the first instance, false if it is a duplicate</returns>
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

    /// <summary>
    /// Unity-like component approach. Iterates through the children of the node starting at the end 
    /// (convention encourages placing component nodes at the bottom of the node's child list to speed up acquisition)
    /// </summary>
    /// <typeparam name="T">The type of component we are trying to acquire. This should be a script attached to a node</typeparam>
    /// <param name="node">The node which we wish to acquire the component of.</param>
    /// <returns>The component if found. Else null</returns>
    public static T? GetComponent<T>(this Node node) where T : Node
    {
        for (int i = node.GetChildCount() - 1; i >= 0; i--)
        {
            if (node.GetChildOrNull<T>(i) is T result) return result; // early quit
        }
        return null;
    }

    public static void RemoveAllChildren(this Node node)
    {
        while (node.GetChildCount() > 0)
        {
            var child = node.GetChild(0);
            child.QueueFree();
            node.RemoveChild(child);
        }
    }

}