using System;
using System.IO;
using System.Text.Json;
using Godot;
using Microsoft.VisualBasic;

public class WorldFile
{
    public const string WldDir = "user://Worlds";
    public const string ChunksDir = "Chunks";

    static public void LoadOrCreate(string wld_name, Main main)
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
        main.world_random = rand;
    }

    static public void CreateWorld(string wld_name)
    {
        using var f = File.Create(GetWldFilePath(wld_name));
        var rand = new Random();
        var data = new WldData((ulong)rand.NextInt64());
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
}

[Serializable]
public class WldData
{
    public string WorldName { get; set; }
    public ulong Seed { get; set; }
    public ulong RandomState;

    public WldData(ulong seed)
    {
        Seed = seed;
        RandomState = seed;
    }
}
