using System;
using Godot;

namespace queen.events;

public static class Events
{

    public static EventsAudio Audio {get; private set;} = new();
    public static EventsGameplay Gameplay {get; private set;} = new();
    public static EventsUI GUI {get; private set;} = new();

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
    public event Action<string> MarkAbleToInteract;
    public event Action MarkUnableToInteract;

    public void TriggerRequestGUI(Control gui_node) => RequestGUI?.Invoke(gui_node);
    public void TriggerRequestCloseGUI() => RequestCloseGUI?.Invoke();

    public void TriggerRequestSubtitle(string text) => RequestSubtitle?.Invoke(text);
    public void TriggerRequestAlert(string text) => RequestAlert?.Invoke(text);
    public void TriggerAbleToInteract(string text) => MarkAbleToInteract?.Invoke(text);
    public void TriggerUnableToInteract() => MarkUnableToInteract?.Invoke();

}