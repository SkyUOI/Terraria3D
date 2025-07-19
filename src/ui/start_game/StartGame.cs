using Godot;

namespace Terraria3D.ui.start_game;

public partial class StartGame : Control
{
    public override void _Ready()
    {
        base._Ready();
        var preferredLanguage = OS.GetLocaleLanguage();
        TranslationServer.SetLocale(preferredLanguage);
    }
}