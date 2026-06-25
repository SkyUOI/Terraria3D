using Godot;

namespace Terraria3D.ui.start_game;

public partial class StartGameMask : ColorRect
{
    public override void _Ready()
    {
        Show();
        GetTree().CreateTween().TweenProperty(this, "color", new Color(0, 0, 0, 0), 2);
    }
}
