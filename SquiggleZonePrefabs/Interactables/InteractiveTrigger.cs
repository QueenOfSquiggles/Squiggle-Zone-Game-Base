using Godot;
using interaction;
using queen.error;
using System;

public partial class InteractiveTrigger : Area3D, IInteractable
{

    [Signal] public delegate void OnInteractedEventHandler();

    public virtual string GetActiveName()
    {
        return Name;
    }

    public virtual bool Interact()
    {
        EmitSignal(nameof(OnInteracted));
        return true;
    }

    public virtual bool IsActive()
    {
        return true;
    }
}
