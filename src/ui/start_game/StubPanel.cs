using Godot;

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
