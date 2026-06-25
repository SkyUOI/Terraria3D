using Godot;

namespace Terraria3D.ui.start_game;

public partial class MultiPlayerButton : ButtonBase
{
    [Export]
    public UIManager UiManager { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Pressed += OnPressed;
    }

    private void OnPressed()
    {
        UiManager.NavigateTo(UIManager.MenuPanel.Multiplayer);
    }
}
