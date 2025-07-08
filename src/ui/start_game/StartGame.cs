using Godot;
using System;

public partial class StartGame : Control
{
    public override void _Ready()
    {
        base._Ready();
        var preferred_language = OS.GetLocaleLanguage();
        TranslationServer.SetLocale(preferred_language);
    }
}
