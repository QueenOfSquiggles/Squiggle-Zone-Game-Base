using System;
using Godot;
using queen.events;
using queen.extension;

public partial class SaveStationGUI : CenterContainer
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        var pos = GetViewport().GetVisibleRect().Size / 2f;
        Input.WarpMouse(pos);
        GetTree().Paused = true;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GetTree().Paused = false;
            Events.GUI.TriggerRequestCloseGUI();
            this.HandleInput();
        }
    }
}
