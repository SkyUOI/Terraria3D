using Godot;

namespace Terraria3D.ui.start_game;

public partial class Logo : Sprite2D
{
    private Vector2 _originalScale;
    private float _originalRotation;

    public override void _Ready()
    {
        _originalScale = Scale;
        _originalRotation = Rotation;
        Larger();

        if (GD.Randi() % 2 == 0)
            LeftRotate();
        else
            RightRotate();
    }

    private void Larger()
    {
        var tween = CreateTween()
            .TweenProperty(this, "scale", _originalScale * 1.3f, 12)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        tween.Finished += Smaller;
    }

    private void Smaller()
    {
        var tween = CreateTween()
            .TweenProperty(this, "scale", _originalScale, 13)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        tween.Finished += Larger;
    }

    private void LeftRotate()
    {
        var tween = CreateTween().TweenProperty(
            this, "rotation", _originalRotation + Mathf.DegToRad(7), 17);
        tween.Finished += RightRotate;
    }

    private void RightRotate()
    {
        var tween = CreateTween().TweenProperty(
            this, "rotation", _originalRotation - Mathf.DegToRad(7), 17);
        tween.Finished += LeftRotate;
    }
}
