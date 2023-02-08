using Godot;
using queen.data;
using queen.error;
using System;

public partial class TestNPC : CharacterBody3D
{

    public void OnInteractedWith()
    {
        Print.MsgOut(new Msg("Interacted with NPC!!!").Color("orange"));
        Effects.Shake(GetTree(), 20.0f, 1.0f, 0.1f);
    }
}
