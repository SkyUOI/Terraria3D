using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Terraria3D;

public partial class Player : CharacterBody3D
{
    [Export]
    public int Speed = 3;
    [Export]
    public int RotateSenY = 3;
    [Export]
    public float RotateSenX = 0.8f;
    [Export]
    public string PlayerName = "guest";

    Vector3 _direction;

    [Export]
    Main _main;
    [Export]
    ui.main_game_ui.MainGameUi _mainGameUi;

    [Export]
    public int Health = 100;
    [Export]
    public int HealthMax = 100;
    [Export]
    public int Mana = 20;
    [Export]
    public int ManaMax = 20;
    [Export]
    public float JumpVelocity = 8f;

    [Export]
    Camera3D _camera3D;

    [Export]
    public bool Enable;

    public override void _Ready()
    {
        base._Ready();
        _camera3D.Current = false;
    }

    public void StartRunning()
    {
        Enable = true;
        _camera3D.Current = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // _main.CheckAndLoadChunk(Position);
        // GD.Print($"player position: {Position}");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!Enable)
        {
            StartRunning();
            return;
        }
        Move(delta);
        // GD.Print(Utils.GetChunk(Position));
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * RotateSenX));
            _camera3D.RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y * RotateSenY));
            var tmpX = Mathf.Clamp(_camera3D.Rotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            _camera3D.Rotation = new Vector3(tmpX, _camera3D.Rotation.Y, 0);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && !_mainGameUi.PointInUi(mouseButton.GlobalPosition))
            {
                _main.MouseInGame();
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
        // GD.Print(Position);
        MoveAndSlide();
    }
}

[Serializable]
public class PlrData(string name)
{
    public string Name { get; set; } = name;
    public Vector3 Position = Vector3.Zero;
    public int Health = 100;
    public int HealthMax = 100;
    public int Mana = 20;
    public int ManaMax = 20;
    public string Difficulty { get; set; } = "Classic";
    public long PlayTimeSeconds { get; set; } = 0;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("o");
}

public class PlrFile
{
    public const string PlrDir = "user://Players";

    public void CreatePlayer(string name)
    {
        var plrDir = ProjectSettings.GlobalizePath(PlrDir);
        Directory.CreateDirectory(plrDir);
        using var f = File.Create(plrDir.PathJoin(name + ".plr"));
        var data = new PlrData(name);
        f.Write(JsonSerializer.SerializeToUtf8Bytes(data));
    }

    public void OpenPlayer(string name, Player player)
    {
        Load(name, player);
    }

    public void Load(string name, Player player)
    {
        using var f = File.OpenRead(PlrDir.PathJoin(name + ".plr"));
        var data = JsonSerializer.Deserialize<PlrData>(f);
        player.Position = data.Position;
        player.PlayerName = data.Name;
    }

    /// <summary>List all player save files in user://Players/.</summary>
    public static List<PlrData> ListPlayers()
    {
        var result = new List<PlrData>();
        var dir = ProjectSettings.GlobalizePath(PlrDir);
        if (!Directory.Exists(dir))
            return result;

        foreach (var filePath in Directory.GetFiles(dir, "*.plr"))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<PlrData>(json);
                if (data != null)
                    result.Add(data);
            }
            catch
            {
                // Skip corrupt files
            }
        }
        return result;
    }

    /// <summary>Delete a player save file by name.</summary>
    public static void DeletePlayer(string name)
    {
        var dir = ProjectSettings.GlobalizePath(PlrDir);
        var filePath = Path.Join(dir, name + ".plr");
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>Check if a player save file exists.</summary>
    public static bool PlayerExists(string name)
    {
        var dir = ProjectSettings.GlobalizePath(PlrDir);
        return File.Exists(Path.Join(dir, name + ".plr"));
    }
}