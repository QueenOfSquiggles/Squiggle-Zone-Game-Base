using Godot;
using interaction;
using queen.error;
using System;

public partial class InteractiveTrigger : Area3D, IInteractable
{

    [Export] public bool is_active = true;
    [Export] private string custom_name = "";
    [Signal] public delegate void OnInteractedEventHandler();

    public virtual string GetActiveName()
    {
        return custom_name.Length > 0? custom_name : Name;
    }

    public virtual bool Interact()
    {
        EmitSignal(nameof(OnInteracted));
        return true;
    }

    public virtual bool IsActive()
    {
        return is_active;
    }
}
