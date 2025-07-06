using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class Player : CharacterBody3D
{
    [Export]
    public int Speed = 10;
    [Export]
    public string PlayerName = "guest";

    Vector3 direction;

    [Export]
    Main main;

    public override void _Process(double delta)
    {
        base._Process(delta);
        Move();

    }

    private void Move()
    {
        var direct = Vector3.Zero;
        if (Input.IsActionPressed("move_left"))
        {
            direct.X -= 1;
        }
        if (Input.IsActionPressed("move_right"))
        {
            direct.X += 1;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            direct.Z -= 1;
        }
        if (Input.IsActionPressed("move_back"))
        {
            direct.Z += 1;
        }
        if (Input.IsActionPressed("move_up"))
        {
            direct.Y += 1;
        }
        if (direct.Length() > 0)
        {
            direct = direct.Normalized();
        }
        Velocity = direct * Speed;
        MoveAndSlide();
    }
}

[Serializable]
public class PlrData
{
    public string Name { get; set; }
    public Vector3 Position = Vector3.Zero;

    public PlrData(string name) => Name = name;
}

public class PlrFile
{
    public const string PlrDir = "user://Players";

    public void CreatePlayer(string name)
    {
        var plr_dir = ProjectSettings.GlobalizePath(PlrDir);
        Directory.CreateDirectory(plr_dir);
        using var f = File.Create(plr_dir.PathJoin(name + ".plr"));
        var data = new PlrData(name);
        f.Write(JsonSerializer.SerializeToUtf8Bytes(data));
    }

    public void OpenPlayer(string name, Player player)
    {
        load(name, player);
    }

    public void load(string name, Player player)
    {
        using var f = File.OpenRead(PlrDir.PathJoin(name + ".plr"));
        var data = JsonSerializer.Deserialize<PlrData>(f);
        player.Position = data.Position;
        player.PlayerName = data.Name;
    }
}
