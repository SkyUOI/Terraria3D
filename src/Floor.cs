using Godot;
using System;

public partial class Floor : MeshInstance3D
{
    public override void _Process(double delta)
    {
        base._Process(delta);
        Position = GetNode<Player>("../Player").Position;
    }
}
