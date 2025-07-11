using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class Player : CharacterBody3D
{
    [Export]
    public int Speed = 10;
    [Export]
    public int rotate_sen = 3;
    [Export]
    public string PlayerName = "guest";

    Vector3 direction;

    [Export]
    Main main;
    [Export]
    MainGameUi main_game_ui;

    [Export]
    public int Health = 100;
    [Export]
    public int HealthMax = 100;
    [Export]
    public int Mana = 20;
    [Export]
    public int ManaMax = 20;

    public override void _Process(double delta)
    {
        base._Process(delta);
        Move();
        main.CheckAndLoadChunk(Position);
        // GD.Print($"player position: {Position}");
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * rotate_sen));
            RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y * rotate_sen));
            var tmpx = Mathf.Clamp(Rotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            var tmpy = Mathf.Clamp(Rotation.Y, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            Rotation = new Vector3(tmpx, tmpy, 0);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && !main_game_ui.PointInUI(mouseButton.GlobalPosition))
            {
                main.MouseInGame();
            }
        }
    }


    private void Move()
    {
        var direct = Vector3.Zero;
        if (Input.IsActionPressed("move_left"))
        {
            direct -= Transform.Basis.X;
        }
        if (Input.IsActionPressed("move_right"))
        {
            direct += Transform.Basis.X;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            direct -= Transform.Basis.Z;
        }
        if (Input.IsActionPressed("move_back"))
        {
            direct += Transform.Basis.Z;
        }
        if (Input.IsActionPressed("move_up"))
        {
            direct += Transform.Basis.Y;
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
    public int Health = 100;
    public int HealthMax = 100;
    public int Mana = 20;
    public int ManaMax = 20;

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
