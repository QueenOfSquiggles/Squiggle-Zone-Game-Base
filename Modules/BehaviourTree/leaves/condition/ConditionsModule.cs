#if TOOLS

using Godot;

namespace queen.behaviour_tree;
[Tool]
public static class ConditionsModule
{
    public static string[] GetRegisterTypes()
    {
        return new string[]
        {
            "BTCondition.cs"
        };
    }
}

#endif