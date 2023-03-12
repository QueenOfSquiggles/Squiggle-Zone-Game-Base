using Godot;
using System;

public partial class OptionsMenu : Control
{
    [Export(PropertyHint.File, "*.tscn")] private string main_menu_path;

    private void OnMenuButtonPressed()
    {
        Scenes.LoadSceneAsync(main_menu_path);
    }

}
