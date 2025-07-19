using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
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
    public const int Y = 16;
    public Vector3I pos;
    public Block[,,] blocks = new Block[X, Y, Z];

    public Chunk(Vector3I pos)
    {
        this.pos = pos;
    }

    public Vector3 GetGlobalPos(Vector3I inchunk_pos)
    {
        return new Vector3(pos.X * X + inchunk_pos.X, pos.Y * Y + inchunk_pos.Y, pos.Z * Z + inchunk_pos.Z);
    }

    public (int, int) HeightRange()
    {
        return (pos.Y, pos.Y + Y - 1);
    }
}

public partial class Main : Node3D
{
    Dictionary<Vector3I, Chunk> dir;
    static string WorldPath = "Test";
    public string WorldName { get; set; }
    [Export]
    public Control main_game_ui { get; set; }
    public RandomNumberGenerator world_random;
    [Export]
    public GridMap grid { get; set; }
    [Export]
    Player player { get; set; }
    [Export]
    bool recreate_world { get; set; }

    public int RenderChunkDistance = 12;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();
        InitMeshLibrary();
        WorldGeneration.Init();

        dir = new Dictionary<Vector3I, Chunk>();
        MouseInGame();
        WorldFile.LoadOrCreate(WorldPath, this);
        RenderBlocks();
    }

    public void InitMeshLibrary()
    {
        var lib = new MeshLibrary();
        BlockRegistry.RegisterBlock<Dirt>();
        foreach (var block_kv in BlockRegistry.BlockTypes)
        {
            lib.CreateItem((int)block_kv.Key);
            lib.SetItemMesh((int)block_kv.Key, BlockRegistry.GetMesh(block_kv.Key));
        }
        grid.MeshLibrary = lib;
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

    public void RenderBlock(Vector3I chunk_pos)
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
                    var block_pos = new Vector3I(j, l, k);
                    block_pos.X += chunk_pos.X * Chunk.X;
                    block_pos.Z += chunk_pos.Y * Chunk.Z;
                    grid.SetCellItem(block_pos, (int)block.blockId);
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

    public void LoadChunk(Vector3I chunk_pos)
    {
        var chunk_path = WorldFile.GetChunksPath(WorldPath).PathJoin(WorldFile.GetChunkFIleName(chunk_pos));
        if (File.Exists(chunk_path))
        {
            using var f = File.OpenRead(chunk_path);
            var chunk_data = JsonSerializer.Deserialize<Chunk>(f);
            dir.Add(chunk_pos, chunk_data);
            return;
        }
        var chunk = new Chunk(chunk_pos);
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

    public void _on_unload_chunks_timer_timeout()
    {
        foreach (var chunk in dir)
        {
            var chunk_pos = chunk.Key;
            if (chunk_pos.DistanceTo(Utils.GetChunk(player.Position)) > RenderChunkDistance)
            {
                dir.Remove(chunk_pos);
            }
        }
    }
}
