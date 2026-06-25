using Godot;

namespace Terraria3D.ui.start_game;

public partial class ButtonBase : Button
{
    private Vector2 _initScale;

    public override void _Ready()
    {
        _initScale = Scale;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        PivotOffset = Size / 2;
    }

    private void OnMouseEntered()
    {
        GetTree().CreateTween()
            .TweenProperty(this, "scale", _initScale * 1.4f, 0.15)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
    }

    private void OnMouseExited()
    {
        GetTree().CreateTween()
            .TweenProperty(this, "scale", _initScale, 0.15)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
    }
}
