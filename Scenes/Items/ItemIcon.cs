using System;
using Godot;
using queen.extension;
using queen.items;

public partial class ItemIcon : TextureRect
{
    [Export] private float SelectOverlayTweenTime = 0.1f;
    [Export] private NodePath pathCountLabel;
    [Export] private NodePath pathSelectOverlay;
    private Label CountLabel;
    private NinePatchRect SelectOverlay;

    private Color COLOR_HIDDEN = new(Colors.White, 0.0f);
    private Color COLOR_VISIBLE = new(Colors.White, 0.75f);

    public override void _Ready()
    {
        this.GetSafe(pathCountLabel, out CountLabel);
        this.GetSafe(pathSelectOverlay, out SelectOverlay);
        SelectOverlay.SelfModulate = COLOR_HIDDEN;
    }

    public void UpdateIcon(ItemStack stack)
    {
        Texture = stack.StackItem.GetItemTexture();
        TooltipText = stack.StackItem.ItemKey;
        if (stack.IsStackable)
            CountLabel.Text = $"x{stack.StackCount}";
        else
            CountLabel.Text = "";
    }

    private void OnMouseEnter() => TweenSelectOverlayColourTo(COLOR_VISIBLE);

    private void OnMouseExit() => TweenSelectOverlayColourTo(COLOR_HIDDEN);

    private void TweenSelectOverlayColourTo(Color color)
    {
        var tween = GetTree().CreateTween().SetDefaultStyle();
        tween.TweenProperty(SelectOverlay, "self_modulate", color, SelectOverlayTweenTime);
    }
}
