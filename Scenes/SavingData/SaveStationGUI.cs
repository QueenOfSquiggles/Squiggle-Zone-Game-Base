using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Godot;
using queen.data;
using queen.error;
using queen.events;
using queen.extension;

public partial class SaveStationGUI : Control
{

    [Export] private NodePath PathSaveSlotContainer;
    [Export] private NodePath PathDeleteConfirmationDialog;
    [Export] private NodePath PathRenameConfirmationDialog;
    [Export] private NodePath PathLabelSlotInfo;
    [Export] private NodePath PathRenameLineEdit;

    private Control SaveSlotContainer;

    private Label LabelSlotInfo;
    private ConfirmationDialog DeleteConfirmDialog;
    private ConfirmationDialog RenameConfirmDialog;
    private LineEdit RenameLineEdit;

    private string current_slot;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        var pos = GetViewport().GetVisibleRect().Size / 2f;
        Input.WarpMouse(pos);
        GetTree().Paused = true;
        this.GetSafe(PathSaveSlotContainer, out SaveSlotContainer);
        this.GetSafe(PathDeleteConfirmationDialog, out DeleteConfirmDialog);
        this.GetSafe(PathRenameConfirmationDialog, out RenameConfirmDialog);
        this.GetSafe(PathRenameLineEdit, out RenameLineEdit);
        this.GetSafe(PathLabelSlotInfo, out LabelSlotInfo);
        PopulateSaveSlots();
    }

    private void PopulateSaveSlots()
    {
        SaveSlotContainer.RemoveAllChildren();

        var current_slots = Data.GetKnownSaveSlots();
        var sorted = new SortedList<DateTime, Button>();
        foreach (var slot in current_slots)
        {
            DateTime date = DateTime.MaxValue;
            string DateInfo = "Default";
            if (slot != "default")
            {
                date = Data.ParseSaveSlotName(slot);
                DateInfo = $"Date {date.ToShortDateString()} --- Time [{date.ToShortTimeString()}]";
            }
            Data.SetSaveSlot(slot);
            string slot_name = Data.CurrentSaveSlot.GetMetaData("slot_name");

            var button = new Button
            {
                // if a slot name is assigned, use that name, else default to date info
                Text = slot_name.Length > 0 ? slot_name : $"{DateInfo}"
            };
            button.SetMeta("slot_key", slot);
            button.Pressed += () => SetCurrentSlot(slot);

            sorted.Add(date, button);
        }

        // add in reverse order so most recent is on top
        var time_order = sorted.Reverse().ToList();
        if (time_order is null) return;

        if (time_order.Count >= 0) SetCurrentSlot(time_order[0].Value.GetMeta("slot_key", "").AsString());
        foreach (var entry in time_order)
        {
            SaveSlotContainer.AddChild(entry.Value);
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GetTree().Paused = false;
            Events.GUI.TriggerRequestCloseGUI();
            this.HandleInput();
        }
    }

    private void SetCurrentSlot(string slot)
    {
        current_slot = slot;
        Data.SetSaveSlot(current_slot);
        Data.CurrentSaveSlot.SaveMetaData();
        LabelSlotInfo.Text = GetSaveSlotMetaDataString();
    }

    private void CreateNewSaveSlot()
    {
        var slotName = Data.CreateSaveSlotName();
        if (slotName is null) return;
        SetCurrentSlot(slotName);
        PopulateSaveSlots();
    }

    private async void SaveDataInSlot()
    {
        Data.SetSaveSlot(current_slot);
        Events.Data.TriggerSerializeAll(); // sends a signal for all serializeable objects to serialize
        await Task.Delay(100); // give some time for signal processing;
        // test saving
        Data.CurrentSaveSlot.SaveText("this is a dumb text file", "test.txt");

        await Task.Delay(100); // give some time for file processing; possibly unnecessary
        PopulateSaveSlots();
    }

    private string GetSaveSlotMetaDataString()
    {
        string slot_name = Data.CurrentSaveSlot.GetMetaData("slot_name");
        string last_access = Data.CurrentSaveSlot.GetMetaData("last_acessed");
        if (DateTime.TryParse(last_access, out DateTime dateInfo))
            last_access = $"{dateInfo.ToShortDateString()} [{dateInfo.ToShortTimeString()}]";
        return $"\"{slot_name}\"\nAccessed: {last_access}";
    }

    private void PromptRenameSlot()
    {
        RenameConfirmDialog.PopupCentered();
    }
    private void PromptDeleteSlot()
    {
        DeleteConfirmDialog.PopupCentered();
    }

    private void OnAcceptRename()
    {
        if (RenameLineEdit.Text.Length <= 0) return;
        Data.CurrentSaveSlot.AddSlotMetaData("slot_name", RenameLineEdit.Text);
        Data.CurrentSaveSlot.SaveMetaData();
        PopulateSaveSlots();
    }

    private void OnAcceptDelete()
    {

        Data.CurrentSaveSlot.DeleteSaveSlot();
        PopulateSaveSlots();
    }
}
