using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using Terraria3D.block.NormalBlock;
using System;
using System.Text.Json.Serialization;
using System.Linq;

namespace Terraria3D;



[Serializable]
public class Region
{
    public float x { get; set; }
    public float y { get; set; }
    public float w { get; set; }
    public float h { get; set; }
    public string source { get; set; }

    public static explicit operator Color(Region region)
    {
        return new Color(region.x, region.y, region.w, region.h);
    }
}

public class Source
{
    public class Size
    {
        public int w { get; set; }
        public int h { get; set; }
    }
    public Size size { get; set; }
}

[Serializable]
public class AtlasData
{
    public Dictionary<string, Source> sources { get; set; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AtlasReceive { get; set; }
    private Dictionary<string, List<Region>> _atlas;
    [JsonIgnore]
    public Dictionary<string, List<Region>> Atlas =>
          _atlas ??= AtlasReceive
                .ToDictionary(
                    kv => kv.Key,
                    kv => JsonSerializer.Deserialize<List<Region>>(kv.Value.ToString())
                );
}

public class SharedData
{
    public static AtlasData AtlasData = JsonSerializer.Deserialize<AtlasData>(File.ReadAllText(ProjectSettings.GlobalizePath("res://resources/tiles/atlas_tiles.json")));
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
    [Export]
    CollisionManager collisionManager;

    public ChunksManager chunksManager = new();


    private int _renderChunkDistance = 9;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();
        WorldGeneration.Init();

        MouseInGame();
        WorldFile.LoadOrCreate(_worldPath, this);
        // RenderBlocks();
        _on_chunks_timer_timeout();
        // Player.StartRunning();
        ChunkTimer.Start();
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

    private void _on_chunks_timer_timeout()
    {
        var playerPos = Player.Position;
        ChunkTimer.Stop();
        var playerChunkPos = Utils.GetChunk(playerPos);
        // remove old chunks
        foreach (var chunk in chunksManager.Chunks)
        {
            var chunkPos = chunk.Key;
            if (OutOfRenderingDistance(playerChunkPos, chunkPos))
            {
                RemoveChunk(chunkPos);
                // GD.Print("Removed chunk: " + chunkPos);
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
                    if (chunksManager.Chunks.ContainsKey(chunkPos))
                    {
                        continue;
                    }
                    AddChunk(chunkPos);
                    // GD.Print($"Loaded Chunk: {chunkPos}");
                }
            }
        }
        ChunkTimer.Start();
    }

    bool OutOfRenderingDistance(Vector3I playerChunkPos, Vector3I chunkPos)
    {
        var tmp = (chunkPos - playerChunkPos).Abs();
        return tmp.X > _renderChunkDistance || tmp.Y > _renderChunkDistance || tmp.Z > _renderChunkDistance;
    }

    void AddChunk(Vector3I chunkPos)
    {
        var generatedchunk = chunksManager.LoadChunk(_worldPath, chunkPos);
        renderer.RenderChunk(generatedchunk);
        // collisionManager.AddCollision(generatedchunk);
    }

    void RemoveChunk(Vector3I chunkPos)
    {
        chunksManager.UnloadChunk(chunkPos);
        renderer.UnrenderChunk(chunkPos);
        collisionManager.RemoveCollision(chunkPos);
    }
}
