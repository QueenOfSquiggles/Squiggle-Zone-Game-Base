#if TOOLS
using Godot;

namespace queen.behaviour_tree;

[Tool]
public static class ActionsModule
{
    public static string[] GetRegisterTypes()
    {
        return new string[]
        {
            "BTAction.cs"
        };
    }
}
#endif