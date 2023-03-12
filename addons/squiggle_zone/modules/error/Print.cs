using System;
using Godot;

namespace queen.error;

public static class Print
{
    public static void Error(string msg)
    {
        GD.PrintErr(msg);
        GD.PushError(msg);
    }

    public static void Warn(string msg)
    {
        GD.PrintErr(msg);
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

    public static void Debug(string msg)
    {
        #if DEBUG
        // Nice little QOL so I don't forget my random print statements
        MsgOut(new Msg($"<DEBUG>: {msg}").Color("light_gray").Italics());
        #endif
    }
}