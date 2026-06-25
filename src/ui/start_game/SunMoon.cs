using Godot;

namespace Terraria3D.ui.start_game;

public partial class SunMoon : Sprite2D
{
    private bool _isSun = true;

    [Export]
    public Texture2D Sun { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> Moons { get; set; }

    private const int MoonFrames = 8;

    public override void _Ready()
    {
        ChangeToSun();
    }

    /// <summary>
    /// Called from .tscn signal connection: screen_exited -> _on_visible_on_screen_notifier_2d_screen_exited
    /// </summary>
    private void _on_visible_on_screen_notifier_2d_screen_exited()
    {
        if (_isSun)
            ChangeToMoon();
        else
            ChangeToSun();
        _isSun = !_isSun;
    }

    private void ChangeToSun()
    {
        Texture = Sun;
        Vframes = 1;
    }

    private void ChangeToMoon()
    {
        Vframes = MoonFrames;
        Texture = Moons[(int)(GD.Randi() % Moons.Count)];
    }
}
