using Godot;

namespace Terraria3D.ui.start_game;

public partial class StartGame : Control
{
    [Export]
    public bool SunManual { get; set; } = false;
    [Export]
    public double DayTime { get; set; } = 0;

    public override void _Ready()
    {
        base._Ready();
        var preferredLanguage = OS.GetLocaleLanguage();
        TranslationServer.SetLocale(preferredLanguage);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!SunManual)
        {
            DayTime += delta * (1.0 / 15) * Consts.DayTime;
        }
    }
}