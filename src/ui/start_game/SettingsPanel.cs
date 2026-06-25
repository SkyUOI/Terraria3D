using Godot;

namespace Terraria3D.ui.start_game;

/// <summary>
/// Settings panel controller. Bridges directly to StartGame (C#) instead of via Call().
/// </summary>
public partial class SettingsPanel : Control
{
    [Export]
    public UIManager UIManager { get; set; }
    [Export]
    public StartGame StartGame { get; set; }

    public override void _Ready()
    {
        Hide();
        UIManager.RegisterPanel(UIManager.MenuPanel.Settings, this);
        LoadSettings();
        ConnectSignals();
        PopulateKeybindings();
    }

    private void LoadSettings()
    {
        var fullscreenCheck = Find<CheckButton>("FullscreenCheck");
        if (fullscreenCheck != null)
            fullscreenCheck.ButtonPressed = StartGame.GetFullscreen();

        var resOption = Find<OptionButton>("ResolutionOption");
        if (resOption != null)
        {
            PopulateResolutions(resOption);
            var rx = StartGame.GetResolutionX();
            var ry = StartGame.GetResolutionY();
            for (int i = 0; i < resOption.ItemCount; i++)
            {
                if (resOption.GetItemText(i) == $"{rx}x{ry}")
                {
                    resOption.Select(i);
                    break;
                }
            }
        }

        var parallaxSlider = Find<HSlider>("ParallaxSlider");
        if (parallaxSlider != null)
            parallaxSlider.Value = StartGame.GetParallaxPercent();

        var musicSlider = Find<HSlider>("MusicSlider");
        if (musicSlider != null)
            musicSlider.Value = StartGame.GetMusicVolume() * 100.0;

        var soundSlider = Find<HSlider>("SoundSlider");
        if (soundSlider != null)
            soundSlider.Value = StartGame.GetSoundVolume() * 100.0;

        var langOption = Find<OptionButton>("LanguageOption");
        if (langOption != null)
        {
            langOption.AddItem("English", 0);
            langOption.SetItemMetadata(0, "en-US");
            langOption.AddItem("中文 (简体)", 1);
            langOption.SetItemMetadata(1, "zh-Hans");
            var currentLang = StartGame.GetLanguage();
            for (int i = 0; i < langOption.ItemCount; i++)
            {
                if ((string)langOption.GetItemMetadata(i) == currentLang)
                {
                    langOption.Select(i);
                    break;
                }
            }
        }

        var backBtn = Find<Button>("BackButton");
        if (backBtn != null)
        {
            backBtn.Text = Tr("Legacy.LegacyMenu.5");
            backBtn.Pressed += OnBackPressed;
        }
    }

    private void ConnectSignals()
    {
        var fc = Find<CheckButton>("FullscreenCheck");
        if (fc != null) fc.Toggled += OnFullscreenChanged;
        var ro = Find<OptionButton>("ResolutionOption");
        if (ro != null) ro.ItemSelected += OnResolutionChanged;
        var ps = Find<HSlider>("ParallaxSlider");
        if (ps != null) ps.ValueChanged += OnParallaxChanged;
        var ms = Find<HSlider>("MusicSlider");
        if (ms != null) ms.ValueChanged += OnMusicChanged;
        var ss = Find<HSlider>("SoundSlider");
        if (ss != null) ss.ValueChanged += OnSoundChanged;
        var lo = Find<OptionButton>("LanguageOption");
        if (lo != null) lo.ItemSelected += OnLanguageChanged;
    }

    private void PopulateResolutions(OptionButton option)
    {
        option.Clear();
        var resos = new Vector2I[]
        {
            new(1280, 720), new(1600, 900),
            new(1920, 1080), new(2560, 1440),
        };
        for (int i = 0; i < resos.Length; i++)
        {
            var r = resos[i];
            option.AddItem($"{r.X}x{r.Y}", i);
        }
    }

    private void PopulateKeybindings()
    {
        var kbList = Find<VBoxContainer>("KeybindingsList");
        if (kbList == null) return;

        foreach (var child in kbList.GetChildren())
            child.QueueFree();

        var actions = InputMap.GetActions();
        actions.Sort();
        foreach (var actionName in actions)
        {
            var nameStr = actionName.ToString();
            if (nameStr!.StartsWith("ui_") || nameStr == "escape")
                continue;

            var events = InputMap.ActionGetEvents(actionName);
            if (events.Count == 0) continue;

            var label = new Label();
            string keyStr = "";
            foreach (var ev in events)
            {
                if (ev is InputEventKey keyEvent)
                {
                    keyStr = keyEvent.AsText();
                    break;
                }
            }
            if (string.IsNullOrEmpty(keyStr))
                keyStr = "(gamepad)";

            label.Text = $"{nameStr.Capitalize()}: {keyStr}";
            label.AddThemeFontSizeOverride("font_size", 18);
            kbList.AddChild(label);
        }
    }

    // ── Signal handlers ──────────────────────────────────────────

    private void OnFullscreenChanged(bool toggled)
    {
        StartGame.SetFullscreen(toggled);
    }

    private void OnResolutionChanged(long idx)
    {
        var ro = Find<OptionButton>("ResolutionOption");
        if (ro == null) return;
        var parts = ro.GetItemText((int)idx).Split('x');
        if (parts.Length == 2)
            StartGame.SetResolution(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private void OnParallaxChanged(double value)
    {
        StartGame.SetParallaxPercent((int)value);
    }

    private void OnMusicChanged(double value)
    {
        StartGame.SetMusicVolume((float)(value / 100.0));
    }

    private void OnSoundChanged(double value)
    {
        StartGame.SetSoundVolume((float)(value / 100.0));
    }

    private void OnLanguageChanged(long idx)
    {
        var lo = Find<OptionButton>("LanguageOption");
        if (lo == null) return;
        StartGame.SetLanguage((string)lo.GetItemMetadata((int)idx));
    }

    private void OnBackPressed()
    {
        UIManager.GoBack();
    }

    /// <summary>
    /// Find a child node by name (non-recursive, not owned). Matches GDScript's find_child().
    /// </summary>
    private T Find<T>(string name) where T : class
    {
        return FindChild(name, false, false) as T;
    }
}
