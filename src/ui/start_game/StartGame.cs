using Godot;

namespace Terraria3D.ui.start_game;

public partial class StartGame : Control
{
    [Export]
    public bool SunManual { get; set; } = false;
    [Export]
    public double DayTime { get; set; } = 0;
    [Export]
    public PlayerChoose PlayerChoose { get; set; }
    [Export]
    public UIManager UIManager { get; set; }


    public override void _Ready()
    {
        base._Ready();
        PlayerChoose.Hide();
        PlayerChoose.UIManager = UIManager;
        // Apply saved settings on startup
        GameSettings.Apply();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!SunManual)
        {
            DayTime += delta * (1.0 / 15) * Constants.DayTime;
        }
    }

    // ── Settings bridge for GDScript ────────────────────────

    public bool GetFullscreen() => GameSettings.Fullscreen;
    public void SetFullscreen(bool value) { GameSettings.Fullscreen = value; ApplyAndSave(); }

    public int GetResolutionX() => GameSettings.Resolution.X;
    public int GetResolutionY() => GameSettings.Resolution.Y;
    public void SetResolution(int x, int y) { GameSettings.Resolution = new Vector2I(x, y); ApplyAndSave(); }

    public int GetParallaxPercent() => GameSettings.ParallaxPercent;
    public void SetParallaxPercent(int value) { GameSettings.ParallaxPercent = value; ApplyAndSave(); }

    public float GetMusicVolume() => GameSettings.MusicVolume;
    public void SetMusicVolume(float value) { GameSettings.MusicVolume = value; ApplyAndSave(); }

    public float GetSoundVolume() => GameSettings.SoundVolume;
    public void SetSoundVolume(float value) { GameSettings.SoundVolume = value; ApplyAndSave(); }

    public string GetLanguage() => GameSettings.Language;
    public void SetLanguage(string value) { GameSettings.Language = value; ApplyAndSave(); }

    public void ApplyAndSave()
    {
        GameSettings.Apply();
        GameSettings.Save();
    }

    // ── Player selection ────────────────────────────────────

    public void OnPlayerSelected(string playerName)
    {
        Main.SelectedPlayerName = playerName;
        GetTree().ChangeSceneToFile("res://src/main.tscn");
    }
}