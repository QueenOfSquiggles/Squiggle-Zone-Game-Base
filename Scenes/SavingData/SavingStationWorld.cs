using System;
using Godot;
using interaction;
using queen.events;

public partial class SavingStationWorld : StaticBody3D, IInteractable
{
    [Export] private string SaveStationName = "Save Station";
    [Export] private PackedScene SaveStationGUIScene;
    public string GetActiveName()
    {
        return SaveStationName;
    }

    public bool Interact()
    {
        var gui = SaveStationGUIScene.InstantiateOrNull<Control>();
        if (gui is null) return false;
        Events.GUI.TriggerRequestGUI(gui);
        return true;
    }

    public bool IsActive()
    {
        return true;
    }
}
