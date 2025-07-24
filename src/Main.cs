using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using System;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;

namespace Terraria3D;



[Serializable]
public class Region
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    [JsonPropertyName("y")]
    public float Y { get; set; }
    [JsonPropertyName("w")]
    public float W { get; set; }
    [JsonPropertyName("h")]
    public float H { get; set; }
    [JsonPropertyName("source")]
    public string Source { get; set; }

    public static explicit operator Color(Region region)
    {
        return new Color(region.X, region.Y, region.H, region.W);
    }
}

public class Source
{
    public class SizeProperty
    {
        [JsonPropertyName("w")]
        public int W { get; set; }
        [JsonPropertyName("h")]
        public int H { get; set; }
    }
    [JsonPropertyName("size")]
    public SizeProperty Size { get; set; }
}

[Serializable]
public class AtlasData
{
    [JsonPropertyName("sources")]
    public Dictionary<string, Source> Sources { get; set; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AtlasReceive { get; set; }
    private Dictionary<int, List<Region>> _atlas;
    [JsonIgnore]
    public Dictionary<int, List<Region>> Atlas =>
          _atlas ??= AtlasReceive
                .ToDictionary(
                    kv => kv.Key.ToInt(),
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
    Timer ChunkTimer { get; set; }
    [Export]
    bool RecreateWorld { get; set; }
    [Export]
    Renderer _renderer;
    [Export]
    CollisionManager _collisionManager;

    public ChunksManager ChunksManager = new();


    private int _renderChunkDistance = 9;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();
        RenderShaderResources.Preload();
        WorldGeneration.Init();

        MouseInGame();
        WorldFile.LoadOrCreate(_worldPath, this);
        _on_chunks_timer_timeout();
        // RenderBlocks();
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
        foreach (var chunk in ChunksManager.Chunks)
        {
            var chunkPos = chunk.Key;
            if (OutOfRenderingDistance(playerChunkPos, chunkPos))
            {
                RemoveChunk(chunkPos);
                // GD.Print("Removed chunk: " + chunkPos);
            }
        }
        // load new chunks
        _ = Task.Run(async () =>
        {
            for (int i = -_renderChunkDistance; i <= _renderChunkDistance; ++i)
            {
                for (int j = -_renderChunkDistance; j <= _renderChunkDistance; ++j)
                {
                    for (int k = -_renderChunkDistance; k <= _renderChunkDistance; ++k)
                    {
                        var chunkPos = new Vector3I(playerChunkPos.X + i, playerChunkPos.Y + j, playerChunkPos.Z + k);
                        if (ChunksManager.Chunks.ContainsKey(chunkPos))
                        {
                            continue;
                        }
                        var generatedChunk = await ChunksManager.LoadChunk(_worldPath, chunkPos);
                        // GD.Print($"Loaded Chunk: {chunkPos}");
                    }
                }
            }
            for (int i = -_renderChunkDistance; i <= _renderChunkDistance; ++i)
            {
                for (int j = -_renderChunkDistance; j <= _renderChunkDistance; ++j)
                {
                    for (int k = -_renderChunkDistance; k <= _renderChunkDistance; ++k)
                    {
                        var chunkPos = new Vector3I(playerChunkPos.X + i, playerChunkPos.Y + j, playerChunkPos.Z + k);
                        if (!ChunksManager.Chunks.TryGetValue(chunkPos, out var generatedChunk))
                        {
                            continue;
                        }
                        // GD.Print($"Loaded Chunk: {chunkPos}");
                        await _renderer.RenderChunk(generatedChunk);
                        await _collisionManager.AddCollision(generatedChunk);
                    }
                }
            }
            ChunkTimer.CallDeferred(Timer.MethodName.Start);
        });
    }

    bool OutOfRenderingDistance(Vector3I playerChunkPos, Vector3I chunkPos)
    {
        var tmp = (chunkPos - playerChunkPos).Abs();
        return tmp.X > _renderChunkDistance || tmp.Y > _renderChunkDistance || tmp.Z > _renderChunkDistance;
    }
    void RemoveChunk(Vector3I chunkPos)
    {
        ChunksManager.UnloadChunk(chunkPos);
        _renderer.UnrenderChunk(chunkPos);
        _collisionManager.RemoveCollision(chunkPos);
    }
}
