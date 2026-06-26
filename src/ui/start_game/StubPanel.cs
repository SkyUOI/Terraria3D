using Godot;
using Terraria3D.achievements;

namespace Terraria3D.ui.start_game;

/// <summary>
/// Reusable stub panel for placeholder screens (Multiplayer, Achievements, Workshop, Credits).
/// </summary>
public partial class StubPanel : Control
{
    [Export]
    public UIManager UIManager { get; set; }
    [Export]
    public Button BackButton { get; set; }

    public override void _Ready()
    {
        Hide();

        // Register with UIManager based on own node name
        switch (Name)
        {
            case "MultiplayerPanel":
                UIManager.RegisterPanel(UIManager.MenuPanel.Multiplayer, this);
                break;
            case "AchievementsPanel":
                UIManager.RegisterPanel(UIManager.MenuPanel.Achievements, this);
                break;
            case "WorkshopPanel":
                UIManager.RegisterPanel(UIManager.MenuPanel.Workshop, this);
                break;
            case "CreditsPanel":
                UIManager.RegisterPanel(UIManager.MenuPanel.Credits, this);
                break;
        }

        if (BackButton != null)
        {
            BackButton.Text = Tr("Legacy.LegacyMenu.5");
            BackButton.Pressed += OnBackPressed;
            BackButton.MouseEntered += () => OnBackHover(BackButton, true);
            BackButton.MouseExited += () => OnBackHover(BackButton, false);
        }

        // Populate achievement list for the AchievementsPanel
        if (Name == "AchievementsPanel")
            BuildAchievementList();
    }

    private void BuildAchievementList()
    {
        // Find the VBoxContainer from the existing scene structure
        var panelContainer = GetNodeOrNull<PanelContainer>("PanelContainer");
        var panel = panelContainer?.GetNodeOrNull<Panel>("Panel");
        var vbox = panel?.GetNodeOrNull<VBoxContainer>("VBoxContainer");
        if (vbox == null) return;

        // Remove "Coming soon..." message
        var message = vbox.GetNodeOrNull<Label>("MessageLabel");
        message?.QueueFree();

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        var list = new VBoxContainer();
        list.AddThemeConstantOverride("separation", 4);
        scroll.AddChild(list);

        var grouped = AchievementManager.GetAllGrouped();

        var summary = new Label
        {
            Text = $"Unlocked: {AchievementManager.UnlockedCount} / {AchievementManager.TotalCount}",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        list.AddChild(summary);
        list.AddChild(new HSeparator());

        foreach (var (category, items) in grouped)
        {
            var header = new Label
            {
                Text = $"-- {AchievementRegistry.CategoryName(category)} --",
            };
            header.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.3f));
            list.AddChild(header);

            foreach (var (id, data, unlocked) in items)
            {
                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 8);

                var icon = new Label { Text = unlocked ? "✓" : "○" };
                icon.AddThemeColorOverride("font_color", unlocked
                    ? new Color(0.3f, 1f, 0.3f)
                    : new Color(0.5f, 0.5f, 0.5f));
                row.AddChild(icon);

                var name = new Label
                {
                    Text = data.Name,
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                };
                if (!unlocked)
                    name.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                row.AddChild(name);

                var desc = new Label { Text = unlocked ? data.Description : "???" };
                if (!unlocked)
                    desc.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
                row.AddChild(desc);

                list.AddChild(row);
            }
            list.AddChild(new HSeparator());
        }

        vbox.AddChild(scroll);
        // Move BackButton to the end
        if (BackButton != null)
            vbox.MoveChild(BackButton, vbox.GetChildCount());
    }

    private void OnBackPressed()
    {
        UIManager.GoBack();
    }

    private void OnBackHover(Button btn, bool entered)
    {
        if (entered)
        {
            var tween = CreateTween();
            tween.TweenProperty(btn, "scale", btn.Scale * 1.15f, 0.12);
        }
        else
        {
            var tween = CreateTween();
            tween.TweenProperty(btn, "scale", Vector2.One, 0.12);
        }
    }
}
