using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using queen.error;

namespace queen.data;

public static class Data
{
    private const string SAVE_SLOT_ROOT = "user://save_slot/";
    private static DataPath DEFAULT_DATA_PATH = new DataPath("user://");

    /// <summary>
    /// An alternate save path pointing towards the current save slot. This can be altered with SetSaveSlot. 
    /// See also: <seealso cref="SetSaveSlot"/>
    /// </summary>
    public static DataPath CurrentSaveSlot { get; private set; } = new(SAVE_SLOT_ROOT + "default/");

    /// <summary>
    /// Checks the root directory of save slots and finds all save slot directories. Meta-data should be collected afterwards.
    /// </summary>
    /// <returns>A string array of save slots, sorted alphabetically </returns>
    public static string[] GetKnownSaveSlots()
    {
        using var dir = DirAccess.Open(SAVE_SLOT_ROOT);
        if (dir == null) return new string[0];
        dir.IncludeHidden = false;
        dir.IncludeNavigational = false;
        return dir.GetDirectories();
    }

    /// <summary>
    /// Sets CurrentSaveSlot to point at a new save slot directory. This will force new files saved to save in that new directory.
    /// See also: <seealso cref="CurrentSaveSlot"/>
    /// </summary>
    /// <param name="slot_name">The name of the file slot. Must be valid for a directory name on the host system. </param>
    public static void SetSaveSlot(string slot_name)
    {
        if (!slot_name.IsValidFileName())
        {
            Print.Error($"'{slot_name}' is not a valid file name. Make sure save slots are being saved using a name that is valid on this system!");
            return;
        }
        CurrentSaveSlot = new($"user://{slot_name}");
    }

    /// <summary>
    /// Saves data in JSON format to the specified path, relative to the root of "user://"
    /// </summary>
    /// <typeparam name="T">inferred type of data</typeparam>
    /// <param name="data">the serializable class to save</param>
    /// <param name="path">the path relative to "user://"</param>
    /// <param name="do_flush">Whether or not to force an IO flush. This is typically unnecessary and will cause a small pause in the game. But forcing a flush does prevent loss of data in the case of a crash. So this is recommended for level save data. </param>
    public static void Save<T>(T data, string path, bool do_flush = false) where T : class => DEFAULT_DATA_PATH.Save<T>(data, path, do_flush);

    /// <summary>
    /// Loads data from a json file to the serializable class.
    /// </summary>
    /// <typeparam name="T">data type to load. Must be specified when it cannot be inferred.</typeparam>
    /// <param name="path">The path of a json file relative to "user://"</param>
    /// <returns>The data of type T that was loaded from file. </returns>
    public static T Load<T>(string path) where T : class => DEFAULT_DATA_PATH.Load<T>(path);

    public static void SetJsonSettings(JsonSerializerOptions options) => DEFAULT_DATA_PATH.SetJsonSettings(options);

}

public class DataPath
{
    public string CurrentPath { get; private set; }
    public JsonSerializerOptions JsonSettings { get; set; } = null;

    public DataPath(string sub_dir)
    {
        CurrentPath = sub_dir;
    }

    /// <summary>
    /// Saves data in JSON format to the specified path
    /// </summary>
    /// <typeparam name="T">inferred type of data</typeparam>
    /// <param name="data">the serializable class to save</param>
    /// <param name="path">the path relative to the assigned path (likely a save slot)</param>
    /// <param name="do_flush">Whether or not to force an IO flush. This is typically unnecessary and will cause a small pause in the game. But forcing a flush does prevent loss of data in the case of a crash. So this is recommended for level save data. </param>
    public void Save<T>(T data, string path, bool do_flush = false) where T : class
    {
        if (JsonSettings == null) LoadDefaultJsonSettings();
        try
        {
            var json_text = JsonSerializer.Serialize(data, JsonSettings);
            SaveText(json_text, path);
        }
        catch (Exception e)
        {
#if DEBUG
            Print.Error($"Failed on JSON serialization process for '{data}' type='{data.GetType().FullName}'.\n\tPath={path}\n\tError: {e.Message}");
#endif
        }
    }
    /// <summary>
    /// Loads data from a json file to the serializable class.
    /// </summary>
    /// <typeparam name="T">data type to load. Must be specified when it cannot be inferred.</typeparam>
    /// <param name="path">The path of a json file relative to the assigned path (likely a save slot)</param>
    /// <returns>The data of type T that was loaded from file. </returns>
    public T? Load<T>(string path) where T : class
    {
        if (JsonSettings == null) LoadDefaultJsonSettings();
        try
        {
            var json_text = LoadText(path);
            if (json_text.EndsWith("}}")) json_text = json_text.Replace("}}", "}");
            var data = JsonSerializer.Deserialize<T>(json_text, JsonSettings);
            return data;
        }
        catch (Exception e)
        {
#if DEBUG
            Print.Error($"Failed on JSON serialization process for type '{typeof(T).FullName}'.\n\tPath={path}\n\tError: {e.Message}");
#endif
        }
        return null;
    }

    public void SaveText(string text, string path, bool do_flush = false)
    {
        try
        {
            path = CurrentPath + path;
            // Print.Debug($"Saving Text:\nDir: {path.GetBaseDir()}\nFile: {path.GetFile()}");
            EnsureDirectoryPaths(path);
            { // deletes the original file to ensure no degenerate files are generated
                using var dir = DirAccess.Open(path.GetBaseDir());
                if (dir is not null && dir.FileExists(path.GetFile())) dir.Remove(path.GetFile());
            }
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file is null)
            {
                Print.Warn($"Failed to open file at path\n\t{path}\n\tError Code #{FileAccess.GetOpenError()}");
                return;
            }

            file.StoreString(text);
            if (do_flush) file.Flush(); // forces a disk write. But also slows down performance
        }
        catch (Exception e)
        {
#if DEBUG
            Print.Error($"Failed to write data out for 'TEXT' => '{text}'.\n\tPath={path}\n\tError: {e.Message}");
#endif
        }
    }

    public string? LoadText(string path)
    {

        try
        {
            path = CurrentPath + path;
            EnsureDirectoryPaths(path);
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            return file.GetAsText(true);
        }
        catch (Exception e)
        {
#if DEBUG
            Print.Error($"Failed to read file.\n\tPath={path}\n\tError: {e.Message}");
#endif
        }
        return null;
    }

    private void EnsureDirectoryPaths(string file_path)
    {

        var globalPath = ProjectSettings.GlobalizePath(file_path.GetBaseDir());
        if (DirAccess.DirExistsAbsolute(globalPath)) return;

        // Print.Debug($"Creating directory path: {globalPath}");
        var err = DirAccess.MakeDirRecursiveAbsolute(globalPath);
        if (err != Error.Ok)
            Print.Error($"Failed to create folder structure for {globalPath}.\n\tError={err}");
    }

    private void LoadDefaultJsonSettings()
    {
        SetJsonSettings(new JsonSerializerOptions()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            IncludeFields = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        });
    }

    public void SetJsonSettings(JsonSerializerOptions options) => JsonSettings = options;

}