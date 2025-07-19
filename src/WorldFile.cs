using System;
using System.IO;
using System.Text.Json;
using Godot;
using Microsoft.VisualBasic;

public class WorldFile
{
    public const string WldDir = "user://Worlds";
    public const string ChunksDir = "Chunks";

    static public void LoadOrCreate(string wld_name, Terraria3D.Main main)
    {
        var path = GetWorldDataPath(wld_name);
        Directory.CreateDirectory(path);
        path = GetWldFilePath(wld_name);
        FileStream f;
        try
        {
            GD.Print($"loading world at {path}");
            f = File.OpenRead(path);
        }
        catch (FileNotFoundException)
        {
            CreateWorld(wld_name);
            f = File.OpenRead(path);
        }
        var data = JsonSerializer.Deserialize<WldData>(f);
        main.WorldName = data.WorldName;
        var rand = new RandomNumberGenerator();
        rand.State = data.RandomState;
        main.WorldRandom = rand;
        WorldGeneration.noise = new FastNoiseLite();
        WorldGeneration.noise.Seed = (int)data.Seed;
    }

    static public void CreateWorld(string wld_name)
    {
        using var f = File.Create(GetWldFilePath(wld_name));
        var seed = GD.Randi();
        var data = new WldData(seed);
        data.WorldName = wld_name;
        f.Write(JsonSerializer.SerializeToUtf8Bytes(data));
    }

    static public void DeleteWorld(string wld_name)
    {
        Directory.Delete(GetWorldDataPath(wld_name), true);
        File.Delete(GetWldFilePath(wld_name));
    }

    static public string GetWorldDataPath(string world_name)
    {
        return ProjectSettings.GlobalizePath(WldDir.PathJoin(world_name));
    }

    static public string GetWldFilePath(string world_name)
    {
        return ProjectSettings.GlobalizePath(WldDir.PathJoin(world_name + ".wld"));
    }

    static public string GetChunksPath(string world_name)
    {
        return GetWorldDataPath(world_name).PathJoin(ChunksDir);
    }

    static public string GetChunkFIleName(Vector3I chunk_pos)
    {
        return chunk_pos.X + "_" + chunk_pos.Y + "_" + chunk_pos.Z + ".chunk";
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
