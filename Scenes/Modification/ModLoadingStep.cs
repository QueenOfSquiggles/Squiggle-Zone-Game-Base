using System;
using Godot;
using queen.data;

public partial class ModLoadingStep : Node
{

    // In a debug context, do not load any modification data. Ideally the modifications someone is developing are already embedded into the project files. And loading will only happen for exported game versions
#if !DEBUG

    public override void _Ready()
    {
        if (OS.HasFeature("demo")) return;
        // Mod loading is not supported for demo builds. 
        // This is explicitly intended to prevent modifications that would gain access to hidden content during demo play.
        // Full releases, usually paid versions, allow full unrestriced access to mod loading because once you've bought the game IDGAF what you do with it. Make an anime titty asset swap for all I care. I'm not responsible for what you choose to add to the game.
        // I may need to make a small tool for mod loading however

        ModRegistry.LoadModsRecursively();
        ModRegistry.OnRegisterMods();
    }

    public override void _ExitTree()
    {
        ModRegistry.OnUnRegisterMods();
    }

#endif
}
