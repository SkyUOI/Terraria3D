using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Terraria3D;

public class Block(BlockId blockId)
{
    public BlockId BlockId = blockId;
}

public class Chunk(Vector3I pos)
{
    public const int X = 16;
    public const int Z = 16;
    public const int Y = 16;
    public Vector3I Pos = pos;
    public Block[,,] Blocks = new Block[X, Y, Z];

    public Vector3 GetGlobalPos(Vector3I inChunkPos)
    {
        return new Vector3(Pos.X * X + inChunkPos.X, Pos.Y * Y + inChunkPos.Y, Pos.Z * Z + inChunkPos.Z);
    }

    public (int, int) HeightRange()
    {
        return (Pos.Y, Pos.Y + Y - 1);
    }
}

public partial class Main : Node3D
{
    Dictionary<Vector3I, Chunk> _dir;
    static string _worldPath = "Test";
    public string WorldName { get; set; }
    [Export]
    public Control MainGameUi { get; set; }
    public RandomNumberGenerator WorldRandom;
    [Export]
    public GridMap Grid { get; set; }
    [Export]
    Player Player { get; set; }
    [Export]
    bool RecreateWorld { get; set; }

    private int _renderChunkDistance = 12;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();
        InitMeshLibrary();
        WorldGeneration.Init();

        _dir = new Dictionary<Vector3I, Chunk>();
        MouseInGame();
        WorldFile.LoadOrCreate(_worldPath, this);
        RenderBlocks();
    }

    private void InitMeshLibrary()
    {
        var lib = new MeshLibrary();
        BlockRegistry.RegisterBlock<Dirt>();
        foreach (var blockKv in BlockRegistry.BlockTypes)
        {
            lib.CreateItem((int)blockKv.Key);
            lib.SetItemMesh((int)blockKv.Key, BlockRegistry.GetMesh(blockKv.Key));
            var shape = BlockRegistry.GetShape(blockKv.Key);
            if (shape != null)
            {
                // GD.Print($"set shape for {block_kv.Key}");
                // GD.Print(shape);
                lib.SetItemShapes((int)blockKv.Key, shape);
            }
        }
        Grid.MeshLibrary = lib;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionPressed("escape"))
        {
            MouseOutGame();
        }
    }

    private void RenderBlocks()
    {
        foreach (var chunkKv in _dir)
        {
            RenderBlock(chunkKv.Key);
        }
    }

    private void RenderBlock(Vector3I chunkPos)
    {
        GD.Print(chunkPos);
        // return;
        var chunk = _dir[chunkPos];
        for (int j = 0; j < Chunk.X; ++j)
        {
            for (int k = 0; k < Chunk.Z; ++k)
            {
                for (int l = 0; l < Chunk.Y; ++l)
                {
                    var block = chunk.Blocks[j, l, k];
                    if (block == null)
                    {
                        continue;
                    }
                    var blockPos = new Vector3I(j, l, k);
                    blockPos.X += chunkPos.X * Chunk.X;
                    blockPos.Z += chunkPos.Y * Chunk.Z;
                    Grid.SetCellItem(blockPos, (int)block.BlockId);
                }
            }
        }
    }

    public void CheckAndLoadChunk(Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (_dir.ContainsKey(chunkPos))
        {
            return;
        }
        LoadChunk(chunkPos);
        RenderBlock(chunkPos);
        // GD.Print($"Loaded Chunk: {dir}");
    }

    private void LoadChunk(Vector3I chunkPos)
    {
        var chunkPath = WorldFile.GetChunksPath(_worldPath).PathJoin(WorldFile.GetChunkFIleName(chunkPos));
        if (File.Exists(chunkPath))
        {
            using var f = File.OpenRead(chunkPath);
            var chunkData = JsonSerializer.Deserialize<Chunk>(f);
            _dir.Add(chunkPos, chunkData);
            return;
        }
        var chunk = new Chunk(chunkPos);
        WorldGeneration.GenerateChunk(chunk);
        _dir.Add(chunkPos, chunk);
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

    private void MouseOutGame()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void _on_unload_chunks_timer_timeout()
    {
        foreach (var chunk in _dir)
        {
            var chunkPos = chunk.Key;
            if (chunkPos.DistanceTo(Utils.GetChunk(Player.Position)) > _renderChunkDistance)
            {
                _dir.Remove(chunkPos);
            }
        }
    }
}