using System;
using System.Threading.Tasks;
using Godot;
using queen.events;

public partial class OptionsMenu : Control
{
    [Export(PropertyHint.File, "*.tscn")] private string main_menu_path;

    private async void OnMenuButtonPressed()
    {
        Events.Data.TriggerSerializeAll();
        await Task.Delay(50); // nice clean 50ms for serializing
        Scenes.LoadSceneAsync(main_menu_path);
    }

}
