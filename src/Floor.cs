using Godot;
using System;

public partial class Floor : MeshInstance3D
{
    [Export]
    Player player;

    public override void _Process(double delta)
    {
        base._Process(delta);
        var tmp = Position;
        tmp.X = player.Position.X;
        tmp.Z = player.Position.Z;
        Position = tmp;
    }
}
