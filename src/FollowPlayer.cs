using Godot;
using System;

public partial class FollowPlayer : Camera3D
{
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Position = GetNode<Player>("../Player").Position;
    }
}
