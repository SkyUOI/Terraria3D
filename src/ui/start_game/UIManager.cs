using System.Collections.Generic;
using Godot;

namespace Terraria3D.ui.start_game;

public partial class UIManager : Node
{
    public enum MenuPanel
    {
        Main,
        PlayerChoose,
        Settings,
        Credits,
        Multiplayer,
        Achievements,
        Workshop,
    }

    private readonly List<MenuPanel> _history = new() { MenuPanel.Main };
    private readonly Dictionary<MenuPanel, Control> _panels = new();

    [Export]
    public VBoxContainer MainButtons { get; set; }
    [Export]
    public Control PlayerChoosePanel { get; set; }

    public override void _Ready()
    {
        _panels[MenuPanel.PlayerChoose] = PlayerChoosePanel;
    }

    public void NavigateTo(MenuPanel panel)
    {
        if (_history.Count == 0)
            return;

        var current = _history[^1];

        // Hide current
        if (current == MenuPanel.Main)
        {
            MainButtons.Hide();
        }
        else
        {
            if (_panels.TryGetValue(current, out var curNode) && curNode != null)
                curNode.Hide();
        }

        // Show target
        _history.Add(panel);
        if (_panels.TryGetValue(panel, out var targetNode) && targetNode != null)
            targetNode.Show();
    }

    public void GoBack()
    {
        if (_history.Count <= 1)
            return;

        var current = _history[^1];
        _history.RemoveAt(_history.Count - 1);

        if (_panels.TryGetValue(current, out var currentPanel) && currentPanel != null)
            currentPanel.Hide();

        var previous = _history[^1];
        if (previous == MenuPanel.Main)
        {
            MainButtons.Show();
        }
        else
        {
            if (_panels.TryGetValue(previous, out var prevPanel) && prevPanel != null)
                prevPanel.Show();
        }
    }

    public void RegisterPanel(MenuPanel panel, Control node)
    {
        _panels[panel] = node;
    }

    public void ResetToMain()
    {
        foreach (var node in _panels.Values)
        {
            if (node != null && node.Visible)
                node.Hide();
        }
        _history.Clear();
        _history.Add(MenuPanel.Main);
        MainButtons.Show();
    }
}
