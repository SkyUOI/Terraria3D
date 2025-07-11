using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

public class Block
{
    public BlockId blockId = BlockId.Nop;

    public Block(BlockId blockId)
    {
        this.blockId = blockId;
    }
}

public class Chunk
{
    public const int X = 16;
    public const int Z = 16;
    public const int Y = 6400;
    public Block[,,] blocks = new Block[X, Y, Z];
}

public partial class Main : Node3D
{
    Dictionary<Vector2, Chunk> dir;
    static string WorldPath = "Test";
    public string WorldName { get; set; }
    [Export]
    public Control main_game_ui { get; set; }
    public StatefulRandom world_random;
    [Export]
    public PackedScene dirt_scene { get; set; }

    public override void _Ready()
    {
        base._Ready();
        dir = new Dictionary<Vector2, Chunk>();
        MouseInGame();
        WorldFile.LoadOrCreate(WorldPath, this);
        RenderBlocks();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionPressed("escape"))
        {
            MouseOutGame();
        }
    }

    public void RenderBlocks()
    {
        foreach (var chunk_kv in dir)
        {
            RenderBlock(chunk_kv.Key);
        }
    }

    public void RenderBlock(Vector2 chunk_pos)
    {
        GD.Print(chunk_pos);
        // return;
        var chunk = dir[chunk_pos];
        for (int j = 0; j < Chunk.X; ++j)
        {
            for (int k = 0; k < Chunk.Z; ++k)
            {
                for (int l = 0; l < Chunk.Y; ++l)
                {
                    var block = chunk.blocks[j, l, k];
                    if (block == null)
                    {
                        continue;
                    }
                    var block_pos = new Vector3(j, l, k);
                    block_pos.X += chunk_pos.X * Chunk.X;
                    block_pos.Z += chunk_pos.Y * Chunk.Z;
                    var block_node = dirt_scene.Instantiate<Dirt>();
                    AddChild(block_node);
                    block_node.Position = block_pos;
                    block_node.Show();
                }
            }
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
        RenderBlock(chunk_pos);
        // GD.Print($"Loaded Chunk: {dir}");
    }

    public void LoadChunk(Vector2 chunk_pos)
    {
        var chunk_path = WorldFile.GetChunksPath(WorldPath).PathJoin(chunk_pos.X + "_" + chunk_pos.Y + ".chunk");
        if (File.Exists(chunk_path))
        {
            using var f = File.OpenRead(chunk_path);
            var chunk_data = JsonSerializer.Deserialize<Chunk>(f);
            dir.Add(chunk_pos, chunk_data);
            return;
        }
        var chunk = new Chunk();
        WorldGeneration.GenerateChunk(chunk);
        dir.Add(chunk_pos, chunk);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        MouseOutGame();
    }

    public void MouseInGame()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void MouseOutGame()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}
