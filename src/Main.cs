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
    bool RecreateWorld { get; set; }

    private int _renderChunkDistance = 9;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();

        MouseInGame();
        WorldFile.LoadOrCreate(_worldPath, this);
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
}
