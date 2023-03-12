using System;
using System.Text.Json;
using Godot;
using queen.error;

namespace queen.data;

public static class Data
{

    public static JsonSerializerOptions JsonSettings {get; set;} = null;

    public static void Save<T>(T data, string path, bool do_flush = false) where T : class
    {
        if (JsonSettings == null) LoadDefaultJsonSettings();
        try
        {
            EnsureDirectoryPaths(path);
            { // deletes the original file to ensure no degenerate files are generated
                using var dir = DirAccess.Open(path.GetBaseDir());
                if (dir.FileExists(path.GetFile())) dir.Remove(path.GetFile());
            }            
            var json_text = JsonSerializer.Serialize(data, JsonSettings);
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file == null) Print.Warn($"Failed to open file at path\n\t{path}\n\tError Code #{FileAccess.GetOpenError()}");
            file.StoreString(json_text);
            if (do_flush) file.Flush(); // forces a disk write. But also slows down performance
        } catch (Exception e)
        {
            #if DEBUG
            Print.Error($"Failed on JSON serialization process for '{data}' type='{data.GetType().FullName}'.\n\tPath={path}\n\tError: {e.Message}");
            #endif
        }
    }

    public static T Load<T>(string path) where T : class
    {
        if (JsonSettings == null) LoadDefaultJsonSettings();
        try
        {
            EnsureDirectoryPaths(path);
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                #if DEBUG
                Print.Error($"Failed on JSON serialization process for type '{typeof(T).FullName}'.\n\tPath={path}\n\tError: {FileAccess.GetOpenError()}");
                #endif
                return null;
            }
            var json_text = file.GetAsText(true);
            if (json_text.EndsWith("}}")) json_text = json_text.Replace("}}", "}");
            var data = JsonSerializer.Deserialize<T>(json_text, JsonSettings);
            return data;
        } catch (Exception e)
        {
            #if DEBUG
            Print.Error($"Failed on JSON serialization process for type '{typeof(T).FullName}'.\n\tPath={path}\n\tError: {e.Message}");
            #endif
        }
        return null;
    }

    private static void EnsureDirectoryPaths(string file_path)
    {
        var global_path = ProjectSettings.GlobalizePath(file_path).GetBaseDir();
        if (DirAccess.DirExistsAbsolute(global_path)) return;
        var err = DirAccess.MakeDirAbsolute(global_path);
        if (err != Error.Ok) Print.Error($"Failed to create folder structure for {file_path}.\n\tError={err}");
    }

    private static void LoadDefaultJsonSettings()
    {
        SetJsonSettings(new JsonSerializerOptions(){
            WriteIndented = true,
            AllowTrailingCommas = true,
            IncludeFields = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        });
    }

    public static void SetJsonSettings(JsonSerializerOptions options) => JsonSettings = options;
    
}