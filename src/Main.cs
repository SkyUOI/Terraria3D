using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using System;
using Terraria3D.achievements;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using Terraria3D.ui.main_game_ui;

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
    public MainGameUi MainGameUi { get; set; }
    public RandomNumberGenerator WorldRandom;
    [Export]
    public Player Player { get; set; }
    [Export]
    bool RecreateWorld { get; set; }

    // --- Entity system ---
    [Export]
    public Node3D Entities { get; set; }
    [Export]
    public entities.spawning.SpawnManager SpawnManager { get; set; }

    /// <summary>Game time in minutes (0–1440). 0 = dawn.</summary>
    public float WorldTime { get; private set; } = 360f; // start at 6:00 AM
    public bool IsNight => WorldTime < 270 || WorldTime > 1170;

    private int _renderChunkDistance = 9;

    /// <summary>Player name passed from the start screen. Set before changing scene.</summary>
    public static string SelectedPlayerName { get; set; } = string.Empty;

    public override void _Ready()
    {
        // if (OS.HasFeature("editor"))
        // {
        // }
        base._Ready();

        // Load player data if a player was selected from start screen
        if (!string.IsNullOrEmpty(SelectedPlayerName))
        {
            var plrFile = new PlrFile();
            plrFile.Load(SelectedPlayerName, Player);
            SelectedPlayerName = string.Empty;
        }

        MouseInGame();
        AchievementManager.Load();
        WorldFile.LoadOrCreate(_worldPath, this);
        MainGameUi.InventoryUI.Player = Player;
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
