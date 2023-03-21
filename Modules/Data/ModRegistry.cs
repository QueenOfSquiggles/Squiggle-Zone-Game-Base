using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;
using queen.error;
using queen.modification;

namespace queen.data;

public static class ModRegistry
{

    private const string MODS_PATH = "user://Mods";
    private static List<IModificationAdapter> mods = new();



    public static void OnRegisterMods()
    {
        foreach (var mod in mods)
            mod.OnRegister();
    }

    public static void OnUnRegisterMods()
    {
        foreach (var mod in mods)
            mod.OnUnRegister();

    }


    public static void LoadModsRecursively()
    {
        var directory = ProjectSettings.GlobalizePath(MODS_PATH);
        using var dir = DirAccess.Open(directory);
        if (dir == null)
        {
            DirAccess.MakeDirRecursiveAbsolute(directory);
            Print.Error($"Failed to load mods during mod step. Error: {DirAccess.GetOpenError()}");
            return;
        }
        dir.IncludeHidden = false;
        dir.IncludeNavigational = false;
        var dirs = dir.GetDirectories();
        foreach (var modDir in dirs)
            LoadModFromDir(directory.PathJoin(modDir));
    }

    private static void LoadModFromDir(string directory)
    {
        using var dir = DirAccess.Open(directory);
        if (dir == null) return;
        string packFile = null;
        string dllFile = null;
        foreach (var file in dir.GetFiles())
        {
            if (file.ToLower().EndsWith("dll")) dllFile = file;
        }
        if (packFile is null) return;

        // DLL is technically optional. Asset swapping doesn't require code. And some small mods could just use GDScript
        if (dllFile is not null) Assembly.LoadFile(directory.PathJoin(dllFile));

        // Load in the pack file. By default files are replaced to allow asset swapping.
        var result = ProjectSettings.LoadResourcePack(packFile);
        if (!result)
        {
            Print.Error($"Failed to load Patch/Mod archive from: {packFile}");
            return;
        }
        Print.Debug($"Loaded modification from {packFile}");
        if (dllFile is not null)
        {
            // Mod has loaded C# code. Find the adapter and get things set up.
            // example adapter path: res://HD_Models_And_Textures/ModAdapter.cs
            var modName = packFile.GetBaseName();
            var modAdapterScriptPath = $"res://{modName}/ModAdapter.cs";
            var script = ResourceLoader.Load(modAdapterScriptPath) as CSharpScript;
            if (script is null)
            {
                Print.Error($"Failed to load C# script from {modAdapterScriptPath}. In order to receive method calls, an adapter must be placed in this exact position for this mod '{modName}'");
                return;
            }
            // LoadAdapterFromPointer(script.NativeInstance, modName);
        }
    }



    // TODO unsafe code must be compiled with the unsafe flag. How to change compile flags for Godot?
    // private unsafe static void LoadAdapterFromPointer(IntPtr ptr, string modName)
    // {
    //     try
    //     {
    //         var objRef = Marshal.PtrToStructure(ptr, typeof(IModificationAdapter));
    //         if (objRef is IModificationAdapter mod)
    //         {
    //             mods.Add(mod);
    //             Print.Warn($"C# Context has been found for mod '{modName}'. Never allow this with code you do not trust.");
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Print.Error($"Failed to load C# Adapter for mod '{modName}'. Error: {e.GetBaseException()}\n---------\n\t{e}");
    //     }

    // }
}