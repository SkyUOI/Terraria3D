using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class Player : CharacterBody3D
{
    [Export]
    public int Speed = 10;
    [Export]
    public int rotate_sen_y = 3;
    [Export]
    public float rotate_sen_x = 0.8f;
    [Export]
    public string PlayerName = "guest";

    Vector3 direction;

    [Export]
    Terraria3D.Main main;
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
    [Export]
    public float JumpVelocity = 4.5f;

    [Export]
    Camera3D camera3D;


    public override void _Process(double delta)
    {
        base._Process(delta);
        main.CheckAndLoadChunk(Position);
        // GD.Print($"player position: {Position}");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Move(delta);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * rotate_sen_x));
            camera3D.RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y * rotate_sen_y));
            var tmpx = Mathf.Clamp(camera3D.Rotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            camera3D.Rotation = new Vector3(tmpx, camera3D.Rotation.Y, 0);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && !main_game_ui.PointInUI(mouseButton.GlobalPosition))
            {
                main.MouseInGame();
            }
        }
    }


    private void Move(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
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
