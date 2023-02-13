using System;
using Godot;

namespace queen.events;

public static class Events
{

    public static EventsAudio Audio => _audio;
    private static EventsAudio _audio = new();

    public static EventsGameplay Gameplay => _gameplay;
    private static EventsGameplay _gameplay = new();

    public static EventsUI GUI => _gui;
    private static EventsUI _gui = new();

}

public class EventsAudio
{
    public event Action<Vector3> OnAudioSpatial;
    public event Action OnAudio;
    public void TriggerOnAudioSpatial(Vector3 position) => OnAudioSpatial?.Invoke(position);
    public void TriggerOnAudio() => OnAudio?.Invoke();
}

public class EventsGameplay
{
    public event Action OnLevelLoaded;
    public event Action OnGameStart;
    public event Action OnPlayerDie;
    public event Action OnPlayerWin;
    public event Action<bool> RequestPlayerAbleToMove;


    public void TriggerOnLevelLoaded() => OnLevelLoaded?.Invoke();
    public void TriggerOnGameStart() => OnGameStart?.Invoke();
    public void TriggerOnPlayerDie() => OnPlayerDie?.Invoke();
    public void TriggerOnPlayerWin() => OnPlayerWin?.Invoke();
    public void TriggerRequestPlayerAbleToMove(bool can_move) => RequestPlayerAbleToMove?.Invoke(can_move);
}

public class EventsUI
{
    public event Action<Control> RequestGUI;
    public event Action RequestCloseGUI;
    public event Action<string> RequestSubtitle;
    public event Action<string> RequestAlert;

    public void TriggerRequestGUI(Control gui_node) => RequestGUI?.Invoke(gui_node);
    public void TriggerRequestCloseGUI() => RequestCloseGUI?.Invoke();

    public void TriggerRequestSubtitle(string text) => RequestSubtitle?.Invoke(text);
    public void TriggerRequestAlert(string text) => RequestAlert?.Invoke(text);

}