using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public class Block
{
    BlockId blockId;
}

public class Chunk
{
    public const int X = 16;
    public const int Z = 16;
    public const int Y = 6400;
    Block[,,] blocks = new Block[X, Y, Z];
}

public partial class Main : Node3D
{
    Dictionary<Vector2, Chunk> dir;
    static string WorldPath = "Test";
    public string WorldName { get; set; }
    [Export]
    public Control main_game_ui { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        WorldFile.LoadOrCreate(WorldPath, this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionPressed("exit"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public void CheckAndLoadChunk(Vector3 pos)
    {
        var chunk_pos = Utils.GetChunk(pos);
        if (dir.ContainsKey(chunk_pos))
        {
            return;
        }
        LoadChunk(chunk_pos);
    }

    public void LoadChunk(Vector2 chunk_pos)
    {
        var chunk = new Chunk();
        dir.Add(chunk_pos, chunk);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}
