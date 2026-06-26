using Godot;
using System.Collections.Generic;
using Terraria3D;
using Terraria3D.ui.start_game;

public partial class PlayerChoose : Control
{
    [Signal]
    public delegate void PlayerSelectedEventHandler(string playerName);

    [Export]
    public VBoxContainer playerList { get; set; }
    private Control _createDialog = null!;
    private LineEdit _nameInput = null!;
    private OptionButton _difficultyOption = null!;
    private Button _playButton = null!;
    private Button _deleteButton = null!;
    private string _selectedPlayerName = string.Empty;
    public UIManager UIManager;

    public void LoadAndShow()
    {
        RefreshPlayerList();
        Show();
    }

    /// <summary>Rebuild the player list from save files.</summary>
    public void RefreshPlayerList()
    {
        // Clear existing entries
        foreach (var child in playerList.GetChildren())
            child.QueueFree();

        _selectedPlayerName = string.Empty;
        UpdateActionButtons();

        var players = PlrFile.ListPlayers();
        if (players.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = Tr("Main.UI.SelectPlayer");
            emptyLabel.AddThemeFontSizeOverride("font_size", 20);
            playerList.AddChild(emptyLabel);
            return;
        }

        foreach (var player in players)
        {
            var btn = new Button();
            btn.Text = $"{player.Name} ({player.Difficulty})";
            btn.SetMeta("player_name", player.Name);
            btn.Flat = true;
            btn.AddThemeFontSizeOverride("font_size", 22);
            btn.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            btn.AddThemeColorOverride("font_hover_color", new Color(1f, 0.84f, 0.31f));
            btn.Pressed += () => OnPlayerClicked(player.Name);
            playerList.AddChild(btn);
        }
    }

    private void OnPlayerClicked(string name)
    {
        _selectedPlayerName = name;
        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        var playBtn = GetNodeOrNull<Button>("PlayButton");
        var deleteBtn = GetNodeOrNull<Button>("DeleteButton");
        var empty = string.IsNullOrEmpty(_selectedPlayerName);
        if (playBtn != null)
            playBtn.Disabled = empty;
        if (deleteBtn != null)
            deleteBtn.Disabled = empty;
    }

    // ── Button handlers ──────────────────────────────────────

    public void OnNewPlayerPressed()
    {
        var dialog = GetNodeOrNull<Control>("CreateDialog");
        if (dialog == null) return;
        dialog.Show();
        var input = dialog.GetNodeOrNull<LineEdit>("NameInput");
        input?.GrabFocus();
    }

    public void OnCreateConfirm()
    {
        var dialog = GetNodeOrNull<Control>("CreateDialog");
        if (dialog == null) return;

        var input = dialog.GetNodeOrNull<LineEdit>("NameInput");
        var name = input?.Text.StripEdges() ?? "";
        if (string.IsNullOrEmpty(name))
            return;

        if (PlrFile.PlayerExists(name))
        {
            GD.Print($"Player '{name}' already exists.");
            return;
        }

        var plrFile = new PlrFile();
        plrFile.CreatePlayer(name);
        dialog.Hide();
        RefreshPlayerList();
    }

    public void OnCreateCancel()
    {
        var dialog = GetNodeOrNull<Control>("CreateDialog");
        dialog?.Hide();
    }

    public void OnDeletePressed()
    {
        if (string.IsNullOrEmpty(_selectedPlayerName))
            return;

        PlrFile.DeletePlayer(_selectedPlayerName);
        _selectedPlayerName = string.Empty;
        RefreshPlayerList();
    }

    public void OnPlayPressed()
    {
        if (string.IsNullOrEmpty(_selectedPlayerName))
            return;

        EmitSignal(SignalName.PlayerSelected, _selectedPlayerName);
    }

    public void OnBackPressed()
    {
        UIManager.GoBack();
    }
}
