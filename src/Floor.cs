using Godot;

namespace Terraria3D;

public partial class Floor : StaticBody3D
{
    [Export]
    Player _player;

    public override void _Process(double delta)
    {
        base._Process(delta);
        var tmp = Position;
        tmp.X = _player.Position.X;
        tmp.Z = _player.Position.Z;
        Position = tmp;
    }
}