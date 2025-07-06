using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public class Chunk
{

}

public partial class Main : Node3D
{
    static Dictionary<(int, int), Chunk> dir;

    public override void _Ready()
    {
        base._Ready();

    }
}
