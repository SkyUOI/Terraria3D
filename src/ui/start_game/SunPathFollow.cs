using Godot;
using System;
using System.Threading.Tasks;
using Terraria3D;
using Terraria3D.ui.start_game;

public partial class SunPathFollow : PathFollow2D
{
    [Export]
    StartGame startGame;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        ProgressRatio = (float)(startGame.DayTime / Consts.DayTime);
    }
}
