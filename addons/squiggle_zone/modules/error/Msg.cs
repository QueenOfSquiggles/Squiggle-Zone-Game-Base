using System;
using Godot;

namespace queen.error;

public class Msg
{
    private string text;

    public Msg(string text)
    {
        this.text = text;
    }

    ///  See Godot.Colors for names (There's a ton of them)
    public Msg Color(string col)
    {
        text = $"[color={col}]{text}[/color]";
        return this;
    }

    public Msg Bold()
    {
        text = $"[b]{text}[/b]";
        return this;
    }
    public Msg Italics()
    {
        text = $"[i]{text}[/i]";
        return this;
    }

    public Msg Underlined()
    {
        text = $"[u]{text}[/u]";
        return this;
    }

    public Msg Monospaced()
    {
        text = $"[code]{text}[/code]";
        return this;
    }

    public Msg BGColour(string colour)
    {
        text = $"[bgcolor={colour}]{text}[/bgcolor]";
        return this;
    }
    public Msg FGColour(string colour)
    {
        text = $"[fgcolor={colour}]{text}[/fgcolor]";
        return this;
    }
    public Msg AlignCenter()
    {
        text = $"[center]{text}[/center]";
        return this;
    }
    public Msg AlignRight()
    {
        text = $"[right]{text}[/right]";
        return this;
    }
    public Msg Indent()
    {
        text = $"[indent]{text}[/indent]";
        return this;
    }    

    public string GetAsText() => text;
}