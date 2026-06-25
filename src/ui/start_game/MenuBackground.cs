using Godot;

namespace Terraria3D.ui.start_game;

public partial class MenuBackground : Control
{
    [Export]
    public Godot.Collections.Array<Texture2D> FirstDayLayers { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> SecondDayLayers { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> ThirdDayLayers { get; set; }

    [Export]
    public Godot.Collections.Array<Texture2D> FirstNightLayers { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> SecondNightLayers { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> ThirdNightLayers { get; set; }
}
