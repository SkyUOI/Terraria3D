using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace Terraria3D;

public class WorldFile
{
    public const string WldDir = "user://Worlds";
    public const string ChunksDir = "Chunks";

    /// <summary>
    /// Load world data from a file, or create a new one if it can't be found.
    /// </summary>
    /// <param name="wldName">Name of the world.</param>
    /// <param name="main">Main godot node.</param>
    public static void LoadOrCreate(string wldName, Main main)
    {
        var path = GetWorldDataPath(wldName);
        Directory.CreateDirectory(path);
        path = GetWldFilePath(wldName);
        FileStream f;
        try
        {
            GD.Print($"loading world at {path}");
            f = File.OpenRead(path);
        }
        catch (FileNotFoundException)
        {
            CreateWorld(wldName);
            f = File.OpenRead(path);
        }
        var data = JsonSerializer.Deserialize<WldData>(f);
        main.WorldName = data.WorldName;
        var rand = new RandomNumberGenerator
        {
            State = data.RandomState
        };
        main.WorldRandom = rand;
        WorldGeneration.Noise = new FastNoiseLite
        {
            Seed = (int)data.Seed
        };
    }

    static public void CreateWorld(string wldName)
    {
        // create the file itself
        using var f = File.Create(GetWldFilePath(wldName));
        var seed = GD.Randi();
        var data = new WldData(seed)
        {
            WorldName = wldName
        };
        f.Write(JsonSerializer.SerializeToUtf8Bytes(data));
        // create regions

    }

    public static void DeleteWorld(string wldName)
    {
        Directory.Delete(GetWorldDataPath(wldName), true);
        File.Delete(GetWldFilePath(wldName));
    }

    public static string GetWorldDataPath(string worldName)
    {
        return ProjectSettings.GlobalizePath(WldDir.PathJoin(worldName));
    }

    public static string GetWldFilePath(string worldName)
    {
        return ProjectSettings.GlobalizePath(WldDir.PathJoin(worldName + ".wld"));
    }

    public static string GetChunksPath(string worldName)
    {
        return GetWorldDataPath(worldName).PathJoin(ChunksDir);
    }

    public static string GetChunkFileName(Vector3I chunkPos)
    {
        return chunkPos.X + "_" + chunkPos.Y + "_" + chunkPos.Z + ".chunk";
    }
}

[Serializable]
public class WldData
{
    public string WorldName { get; set; }
    public uint Seed { get; set; }
    public ulong RandomState;

    public WldData(uint seed)
    {
        Seed = seed;
        var rand = new RandomNumberGenerator();
        rand.Seed = seed;
        RandomState = rand.State;
    }
}