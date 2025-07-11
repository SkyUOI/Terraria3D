using System;
using System.IO;
using System.Text.Json;
using Godot;

public class WorldFile
{
    public const string WldDir = "user://Worlds";
    public const string ChunksDir = "Chunks";

    static public void LoadOrCreate(string wld_name, Main main)
    {
        var path = ProjectSettings.GlobalizePath(WldDir.PathJoin(wld_name));
        Directory.CreateDirectory(path);
        path = ProjectSettings.GlobalizePath(WldDir.PathJoin(wld_name + ".wld"));
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
        main.world_random = new StatefulRandom(data.RandomState);
    }

    static public void CreateWorld(string wld_name)
    {
        var wld_dir = ProjectSettings.GlobalizePath(WldDir);
        Directory.CreateDirectory(wld_dir);
        using var f = File.Create(wld_dir.PathJoin(wld_name + ".wld"));
        var rand = new Random();
        var data = new WldData((ulong)rand.NextInt64());
        data.WorldName = wld_name;
        f.Write(JsonSerializer.SerializeToUtf8Bytes(data));
    }

    static public string GetWorldPath(string world_name)
    {
        return ProjectSettings.GlobalizePath(WldDir.PathJoin(world_name));
    }

    static public string GetChunksPath(string world_name)
    {
        return GetWorldPath(world_name).PathJoin(ChunksDir);
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
