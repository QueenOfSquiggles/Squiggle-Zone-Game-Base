using Godot;

namespace queen.error;

public static class Print
{
    public static void Error(string msg)
    {
        GD.PrintRich($"[b][color=red]{msg}[/color][/b]");
        GD.PushError(msg);
    }

    public static void Warn(string msg)
    {
        GD.PrintRich($"[b][color=yellow]{msg}[/color][/b]");
        GD.PushWarning(msg);
    }
    public static void Info(string msg)
    {
        Colour(msg, "blue");
    }

    public static void Colour(string msg, string colour)
    {
        GD.PrintRich($"[b][color={colour}]{msg}[/color][/b]");
    }
    public static void MsgOut(Msg message)
    {
        GD.PrintRich(message.GetAsText());
    }

}