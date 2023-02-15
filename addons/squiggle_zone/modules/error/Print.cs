using Godot;

namespace queen.error;

public static class Print
{
    public static void Error(string msg)
    {
        Colour(msg, "red");
        GD.PushError(msg);
    }

    public static void Warn(string msg)
    {
        Colour(msg, "yellow");
        GD.PushWarning(msg);
    }
    public static void Info(string msg)
    {
        Colour(msg, "cyan");
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