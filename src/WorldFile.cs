using System;
using System.IO;
using System.Text.Json;
using Godot;

public class WorldFile
{
    public const string WldDir = "user://Worlds";

    static public void LoadOrCreate(string wld_name, Main main)
    {
        var path = ProjectSettings.GlobalizePath(WldDir.PathJoin(wld_name));
        Directory.CreateDirectory(path);
        path = ProjectSettings.GlobalizePath(WldDir.PathJoin(wld_name + ".wld"));
        FileStream f;
        try
        {
            f = File.OpenRead(path);
        }
        catch (FileNotFoundException)
        {
            CreateWorld(wld_name);
            f = File.OpenRead(path);
        }
        var data = JsonSerializer.Deserialize<WldData>(f);
        main.WorldName = data.WorldName;
    }

    static public void CreateWorld(string wld_name)
    {
        var wld_dir = ProjectSettings.GlobalizePath(WldDir);
        Directory.CreateDirectory(wld_dir);
        using var f = File.Create(wld_dir.PathJoin(wld_name + ".wld"));
        var data = new WldData();
        data.WorldName = wld_name;
        f.Write(JsonSerializer.SerializeToUtf8Bytes(data));
    }
}

[Serializable]
public class WldData
{
    public string WorldName { get; set; }
}
