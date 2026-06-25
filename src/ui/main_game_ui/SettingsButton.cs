using Godot;

namespace Terraria3D.ui.main_game_ui;

public partial class SettingsButton : Button
{
    private Vector2 _initScale;

    public override void _Ready()
    {
        _initScale = Scale;
    }

    /// <summary>
    /// Called from .tscn signal connection: mouse_entered -> _on_mouse_entered
    /// </summary>
    private void _on_mouse_entered()
    {
        GetTree().CreateTween()
            .TweenProperty(this, "scale", _initScale * 1.2f, 0.2f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
    }

    /// <summary>
    /// Called from .tscn signal connection: mouse_exited -> _on_mouse_exited
    /// </summary>
    private void _on_mouse_exited()
    {
        GetTree().CreateTween()
            .TweenProperty(this, "scale", _initScale, 0.2f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
    }
}
