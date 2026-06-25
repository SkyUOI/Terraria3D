using Godot;

namespace Terraria3D.ui.start_game;

public partial class ExitButton : ButtonBase
{
    public override void _Ready()
    {
        base._Ready();
        Pressed += OnPressed;
    }

    private void OnPressed()
    {
        GetTree().Quit();
    }
}
