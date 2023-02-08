using Godot;
using queen.data;
using queen.error;
using queen.extension;
using System;

public class DataHolder
{
    public int SomeValue {get; set;} = 420;
    public float AnotherValue = 2.316f;
    public int PropShouldSkip {get;} = 20;
    public readonly int FieldShouldSkip = 20;

}
public partial class TestScene : Node
{

    [Export] private string file_path = "user://file.json";

    public override void _Ready()
    {
        Print.Warn("This is a test warning");
        var data = new DataHolder();
        Data.Save(data, file_path);
    }
}
