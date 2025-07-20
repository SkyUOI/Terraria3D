using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Terraria3D.block.NormalBlock;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Terraria3D.item;

namespace Terraria3D;

public class Block(BlockId blockId)
{
    public BlockId BlockId = blockId;
}

[Serializable]
public class Region
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Source { get; set; }

    public static explicit operator Godot.Color(Region region)
    {
        return new Godot.Color(region.X, region.Y, region.Width, region.Height);
    }
}

public class Source
{
    public class Size
    {
        public int W { get; set; }
        public int H { get; set; }
    }

    public class InternalData
    {
        public Dictionary<string, Size> SourceData { get; set; }
    }

    public Dictionary<string, InternalData> Item;
}

[Serializable]
public class AtlasData
{
    public Dictionary<string, Source> sources;
    [JsonExtensionData]
    public Dictionary<string, List<Region>> Atlas;
}

public class SharedData
{
    public static AtlasData AtlasData = JsonSerializer.Deserialize<AtlasData>(File.ReadAllText("res://resources/tiles/atlas_tiles.json"));
}

public partial class Main : Node3D
{
    static string _worldPath = "Test";
    public string WorldName { get; set; }
    [Export]
    public Control MainGameUi { get; set; }
    public RandomNumberGenerator WorldRandom;
    [Export]
    Player Player { get; set; }
    [Export]
    Godot.Timer ChunkTimer { get; set; }
    [Export]
    bool RecreateWorld { get; set; }
    [Export]
    Renderer renderer;

    public ChunksManager chunksManager = new();


    private int _renderChunkDistance = 3;
    private const int MAX_UPDATES_PER_FRAME = 500;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();
        InitMeshLibrary();
        WorldGeneration.Init();

        MouseInGame();
        WorldFile.LoadOrCreate(_worldPath, this);
        // RenderBlocks();
        _on_chunks_timer_timeout();
        // Player.StartRunning();
        ChunkTimer.Start();
    }

    private void InitMeshLibrary()
    {
        // var lib = new MeshLibrary();
        // BlockRegistry.RegisterBlock<Dirt>();
        // foreach (var blockKv in BlockRegistry.BlockTypes)
        // {
        //     lib.CreateItem((int)blockKv.Key);
        //     lib.SetItemMesh((int)blockKv.Key, BlockRegistry.GetShaderData(blockKv.Key));
        //     var shape = BlockRegistry.GetShape(blockKv.Key);
        //     if (shape != null)
        //     {
        //         // GD.Print($"set shape for {block_kv.Key}");
        //         // GD.Print(shape);
        //         lib.SetItemShapes((int)blockKv.Key, shape);
        //     }
        // }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionPressed("escape"))
        {
            MouseOutGame();
        }
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

    private async void _on_chunks_timer_timeout()
    {
        var playerPos = Player.Position;
        ChunkTimer.Stop();
        await Task.Run(() =>
    {
        var playerChunkPos = Utils.GetChunk(playerPos);
        // remove old chunks
        foreach (var chunk in chunksManager.Chunks)
        {
            var chunkPos = chunk.Key;
            if (OutOfRenderingDistance(playerChunkPos, chunkPos))
            {
                chunksManager.UnloadChunk(chunkPos);
                renderer.UnrenderChunk(chunkPos);
                GD.Print("Removed chunk: " + chunkPos);
            }
        }
        // load new chunks
        for (int i = -_renderChunkDistance; i <= _renderChunkDistance; ++i)
        {
            for (int j = -_renderChunkDistance; j <= _renderChunkDistance; ++j)
            {
                for (int k = -_renderChunkDistance; k <= _renderChunkDistance; ++k)
                {
                    var chunkPos = new Vector3I(playerChunkPos.X + i, playerChunkPos.Y + j, playerChunkPos.Z + k);
                    if (!chunksManager.Chunks.TryGetValue(chunkPos, out var chunk))
                    {
                        continue;
                    }
                    chunksManager.LoadChunk(_worldPath, chunkPos);
                    renderer.RenderChunk(chunk);
                    GD.Print($"Loaded Chunk: {chunkPos}");
                }
            }
        }
        // RenderBlocks();
        ChunkTimer.CallDeferred(Godot.Timer.MethodName.Start);
    }).ConfigureAwait(false);
    }

    bool OutOfRenderingDistance(Vector3I playerChunkPos, Vector3I chunkPos)
    {
        var tmp = (chunkPos - playerChunkPos).Abs();
        return tmp.X > _renderChunkDistance || tmp.Y > _renderChunkDistance || tmp.Z > _renderChunkDistance;
    }
}
