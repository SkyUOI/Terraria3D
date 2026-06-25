using Godot;

namespace Terraria3D;

/// <summary>
/// Persistent game settings stored in user://settings.cfg via Godot's ConfigFile.
/// Covers video, audio, and language preferences.
/// </summary>
public static class GameSettings
{
    private const string ConfigPath = "user://settings.cfg";

    // ── Video ──────────────────────────────────────────────
    public static bool Fullscreen { get; set; } = false;
    public static Vector2I Resolution { get; set; } = new(1920, 1080);
    public static bool ParallaxEnabled { get; set; } = true;
    public static int ParallaxPercent { get; set; } = 100;

    // ── Audio ──────────────────────────────────────────────
    public static float MusicVolume { get; set; } = 1.0f;
    public static float SoundVolume { get; set; } = 1.0f;

    // ── General ────────────────────────────────────────────
    public static string Language { get; set; } = "en-US";

    // ── Persistent load / save ─────────────────────────────

    static GameSettings()
    {
        Load();
    }

    public static void Load()
    {
        var config = new ConfigFile();
        Error err = config.Load(ConfigPath);
        if (err != Error.Ok)
        {
            DetectSystemLanguage();
            return;
        }

        Fullscreen       = (bool)config.GetValue("video",   "fullscreen",       false);
        ParallaxEnabled  = (bool)config.GetValue("video",   "parallax_enabled", true);
        ParallaxPercent  = (int)config.GetValue("video",    "parallax_percent", 100);
        var resX         = (int)config.GetValue("video",    "resolution_x",     1920);
        var resY         = (int)config.GetValue("video",    "resolution_y",     1080);
        Resolution       = new Vector2I(resX, resY);

        MusicVolume      = (float)config.GetValue("audio",  "music_volume",     1.0f);
        SoundVolume      = (float)config.GetValue("audio",  "sound_volume",     1.0f);

        Language         = (string)config.GetValue("general", "language",       "");
        if (string.IsNullOrEmpty(Language))
            DetectSystemLanguage();
    }

    public static void Save()
    {
        var config = new ConfigFile();
        config.SetValue("video",   "fullscreen",        Fullscreen);
        config.SetValue("video",   "parallax_enabled",  ParallaxEnabled);
        config.SetValue("video",   "parallax_percent",  ParallaxPercent);
        config.SetValue("video",   "resolution_x",      Resolution.X);
        config.SetValue("video",   "resolution_y",      Resolution.Y);

        config.SetValue("audio",   "music_volume",      MusicVolume);
        config.SetValue("audio",   "sound_volume",      SoundVolume);

        config.SetValue("general", "language",          Language);
        config.Save(ConfigPath);
    }

    // ── Apply settings to the engine ───────────────────────

    public static void Apply()
    {
        // Video
        if (Fullscreen)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowSetSize(Resolution);
        }

        // Audio – try "Master" bus, then index 0 as fallback
        ApplyBusVolume("Master", SoundVolume);
        ApplyBusVolume("Music",  MusicVolume);

        // Language
        if (!string.IsNullOrEmpty(Language))
            TranslationServer.SetLocale(Language);
    }

    private static void ApplyBusVolume(string busName, float linearVolume)
    {
        int idx = AudioServer.GetBusIndex(busName);
        if (idx < 0)
        {
            // Fallback: use bus 0 ("Master") when named buses don't exist
            idx = AudioServer.GetBusIndex("Master");
            if (idx < 0)
                return;
        }
        // linear 0..1 → dB  (-80 .. 0)
        float db = linearVolume <= 0.0001f ? -80f : Mathf.LinearToDb(linearVolume);
        AudioServer.SetBusVolumeDb(idx, db);
    }

    private static void DetectSystemLanguage()
    {
        string sysLang = OS.GetLocaleLanguage();
        Language = sysLang switch
        {
            "zh" or "zh_CN" or "zh-Hans" => "zh-Hans",
            _                             => "en-US",
        };
    }
}
