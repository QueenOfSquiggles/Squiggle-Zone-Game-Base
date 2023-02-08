using Godot;
using queen.data;
using queen.error;
using queen.extension;
using System;
using System.Threading.Tasks;

public partial class ControlsTab : PanelContainer
{

    private bool Listening = false;
    private string CurrentActionTarget = "";
    private Popup popup_listening;

    public override void _Ready()
    {
        this.GetNode("ListeningPopup", out popup_listening);
        popup_listening.Exclusive = true;
        popup_listening.WindowInput += _Input;
    }

    public async void ListenForAction(string action_name)
    {
        CurrentActionTarget = action_name;
        popup_listening.PopupCenteredRatio();
        await Task.Delay(50);
        Listening = true;
    }

    public override void _Input(InputEvent e)
    {
        if (!Listening) return;
        if (CurrentActionTarget.Length == 0) return;
        
        bool is_valid = false;

        // OMFG I'm loving pattern matching!!!
        if (e is InputEventKey key) is_valid = key.Pressed;
        else if (e is InputEventJoypadButton joy) is_valid = joy.Pressed;
        else if (e is InputEventMouseButton mou) is_valid = mou.Pressed;
        else if (e is InputEventJoypadMotion axis) is_valid = Mathf.Abs(axis.AxisValue) > 0.999f; // force max to avoid noise/drift

        if (is_valid)
        {
            Print.MsgOut(new Msg($"Processing input event override for action {CurrentActionTarget}, received event:\n\t{e.AsText()}"));
            Controls.Instance.SetMapping(CurrentActionTarget, e);
            CurrentActionTarget = "";
            Listening = false;
            popup_listening.Hide();
        }
    }

    public void ResetAllMappings()
    {
        Controls.Instance.ResetMappings();
    }

    public void ApplyChanges()
    {
        Controls.SaveSettings();
    }

}
