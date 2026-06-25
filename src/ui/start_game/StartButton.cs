using Godot;

namespace Terraria3D.ui.start_game;

public partial class StartButton : Button
{
    /// <summary>
    /// Called from .tscn signal connection: pressed -> _on_pressed
    /// </summary>
    private void _on_pressed()
    {
        GetTree().ChangeSceneToFile("res://src/main.tscn");
    }
}
