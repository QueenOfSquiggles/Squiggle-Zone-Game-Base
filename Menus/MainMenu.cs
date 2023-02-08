namespace menus;

using Godot;
using queen.extension;
using System;

public partial class MainMenu : Control
{
    [Export] private bool test_pirate = false;
    [Export(PropertyHint.File, "*.tscn")] private string play_scene;
    [Export(PropertyHint.File, "*.tscn")] private string options_scene;
    [Export(PropertyHint.File, "*.tscn")] private string credits_scene;
    [Export] private NodePath path_pirate_popup;

    private AcceptDialog pirate_popup;

    public override void _Ready()
    {
        GD.Print("Loaded main menu scene");
        this.GetNode(path_pirate_popup, out pirate_popup);
        
        if (OS.HasFeature("pirate") || test_pirate)
        {
            pirate_popup.PopupCenteredRatio(0.8f);
            OS.ShellOpen("https://ko-fi.com/queenofsquiggles");
            OS.ShellOpen("https://queenofsquiggles.itch.io/where-the-dead-lie");
        }
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    private void OnBtnPlay() => Scenes.LoadSceneAsync(play_scene);

    private void OnBtnOptions() => Scenes.LoadSceneAsync(options_scene);

    private void OnBtnCredits() => Scenes.LoadSceneAsync(credits_scene);

    private void OnBtnQuit() => GetTree().Quit();


}
