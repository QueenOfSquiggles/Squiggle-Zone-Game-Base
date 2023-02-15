using Godot;
using System;

public partial class DemoEndScene : Control
{
    [Export(PropertyHint.File, "*.tscn")] private string main_menu_scene;

    public void ReturnMainMenu() => Scenes.LoadSceneAsync(main_menu_scene);
}
