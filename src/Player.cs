using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export]
    public int Speed = 5;

    Vector3 direction;

    public override void _Process(double delta)
    {
        base._Process(delta);
        var direct = Vector3.Zero;
        if (Input.IsActionPressed("move_left"))
        {
            direct.X -= 1;
        }
        if (Input.IsActionPressed("move_right"))
        {
            direct.X += 1;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            direct.Z -= 1;
        }
        if (Input.IsActionPressed("move_back"))
        {
            direct.Z += 1;
        }
        if (Input.IsActionPressed("move_up"))
        {
            direct.Y += 1;
        }
        if (direct.Length() > 0)
        {
            direct = direct.Normalized();
        }
        Velocity = direct;
        MoveAndSlide();
    }
}
