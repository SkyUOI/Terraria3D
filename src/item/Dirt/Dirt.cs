using Godot;
using System;
using System.Threading.Tasks;

public partial class Dirt : Node3D
{
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _EnterTree()
    {
        base._EnterTree();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // GD.Print($"spawned dirt at {GlobalPosition}");
    }
}
