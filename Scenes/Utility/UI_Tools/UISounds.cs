using System;
using System.Threading.Tasks;
using Godot;
using queen.data;
using queen.error;
using queen.extension;

public partial class UISounds : Node
{

    [Export] private NodePath path_select_sfx;
    [Export] private NodePath path_click_sfx;

    private AudioStreamPlayer sfx_select;
    private AudioStreamPlayer sfx_click;
    private static string VoiceID = "";

    public override void _Ready()
    {
        this.GetNode(path_select_sfx, out sfx_select);
        this.GetNode(path_click_sfx, out sfx_click);
        var parent = GetParent<Control>();
        if (VoiceID == "") VoiceID = DisplayServer.TtsGetVoicesForLanguage(OS.GetLocaleLanguage())[0];

        if (Debugging.Assert(parent != null, "UISounds node must be child of a Control node!"))
        {
            ConnectSignalsDelayed(parent, 50);
        }
    }

    private async void ConnectSignalsDelayed(Control parent, int delay)
    {
        await Task.Delay(delay);
        if (!IsInstanceValid(parent)) return;
        if (parent is TabContainer tabs)
        {
            tabs.TabButtonPressed += (index) => OnClick();
            return; // don't do select on container. That would get annoying
        }

        parent.FocusEntered += OnSelect;
        parent.MouseEntered += OnSelect;
        if (parent is Button btn)
            btn.Pressed += OnClick;
        else if (parent is HSlider slider)
            slider.DragStarted += OnClick;
        else if (parent is LinkButton link)
            link.Pressed += OnClick;
        else
            Print.Warn($"Unsupported parent type: {parent.GetType()}");

    }

    private void OnSelect()
    {
        if (Access.Instance.ReadVisibleTextAloud && GetParent().Get("text").VariantType != Variant.Type.Nil)
            DoTTS(GetParent().Get("text").AsString());
        sfx_select.Play();
    }
    private void OnClick() => sfx_click.Play();

    private async void DoTTS(string msg)
    {
        if (VoiceID == "") return;
        DisplayServer.TtsStop();
        DisplayServer.TtsSpeak(msg, VoiceID);
    }

    private void GetTTSVoiceID()
    {
        var voices = DisplayServer.TtsGetVoicesForLanguage(OS.GetLocaleLanguage());
        if (voices.Length <= 0)
        {
            Print.Warn($"Warning. No TTS voices installed for host system locale: {OS.GetLocaleLanguage()}. We will not be able to perform TTS lookups. If user wishes to use TTS on UI elements. Please install a TTS voice for your system. More information here: https://docs.godotengine.org/en/stable/tutorials/audio/text_to_speech.html#requirements-for-functionality");
        }
    }

}
