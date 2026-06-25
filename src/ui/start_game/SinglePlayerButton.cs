using Godot;

namespace Terraria3D.ui.start_game;

public partial class SinglePlayerButton : ButtonBase
{
    [Export]
    public PlayerChoose PlayerChoose { get; set; }
    [Export]
    public UIManager UiManager { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Pressed += OnPressed;
    }

    private void OnPressed()
    {
        PlayerChoose.LoadAndShow();
        UiManager.NavigateTo(UIManager.MenuPanel.PlayerChoose);
    }
}
