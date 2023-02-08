using System;
using Godot;
using queen.data;
using queen.error;

public partial class LevelTest : Control
{

    [Export(PropertyHint.File, "*.tscn")] private string load_scene_file = "";
    [Export] private string data_file_path = "user://dir/test.dat";

    [Export] public int DebugData_Int {
        get {
            return data.some_int;
        }
        set {}
    }
    private GameData data = new();
    private readonly Random random = new();

    public void OnButtonPressed()
    {
        Scenes.LoadSceneAsync(load_scene_file);
    }

    public void SaveData()
    {
        data.some_float = random.NextSingle();
        data.some_int = random.Next();
        Data.Save(data, data_file_path, true);
        Print.Colour(data.ToString(), "pink");
    }

    public void LoadData()
    {
        data = Data.Load<GameData>(data_file_path);
        Print.Colour(data.ToString(), "purple");
    }
}
