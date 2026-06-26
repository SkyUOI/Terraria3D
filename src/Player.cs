using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using Terraria3D.buffs;
using Terraria3D.items;

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

    // ── Inventory & Equipment ─────────────────────────────────────────

    /// <summary>Player inventory (50 main + coins + ammo).</summary>
    public Inventory Inventory { get; private set; } = new();

    /// <summary>Player equipment (armor, accessories, vanity, etc.).</summary>
    public Equipment Equipment { get; private set; } = new();

    // ── Buff system ────────────────────────────────────────────────────

    /// <summary>Active buffs and debuffs on this player.</summary>
    public BuffManager Buffs { get; private set; } = new();

    /// <summary>Effective max HP (base + equipment + buffs).</summary>
    public int EffectiveMaxHealth =>
        HealthMax + Equipment.TotalHealthBonus + Buffs.GetCombinedEffects().MaxHealthBonus;

    /// <summary>Effective max MP (base + equipment + buffs).</summary>
    public int EffectiveMaxMana =>
        ManaMax + Equipment.TotalManaBonus + Buffs.GetCombinedEffects().MaxManaBonus;

    /// <summary>Effective defense (base + equipment + buffs).</summary>
    public int EffectiveDefense =>
        Equipment.TotalDefense + Buffs.GetCombinedEffects().DefenseBonus;

    /// <summary>Effective movement speed (base × (1 + equipment% + buffs%)).</summary>
    public float EffectiveSpeed =>
        Speed * (1f + Equipment.TotalSpeedBonus + Buffs.GetCombinedEffects().SpeedMultiplier);

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
        Buffs.Process(delta, this);
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
            // Scroll wheel → cycle hotbar
            else if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                Inventory.SelectedIndex = (Inventory.SelectedIndex + 9) % Inventory.HotbarSize;
            }
            else if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                Inventory.SelectedIndex = (Inventory.SelectedIndex + 1) % Inventory.HotbarSize;
            }
        }
        // Number keys 1-9,0 → select hotbar slot
        else if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Key1: Inventory.SelectedIndex = 0; break;
                case Key.Key2: Inventory.SelectedIndex = 1; break;
                case Key.Key3: Inventory.SelectedIndex = 2; break;
                case Key.Key4: Inventory.SelectedIndex = 3; break;
                case Key.Key5: Inventory.SelectedIndex = 4; break;
                case Key.Key6: Inventory.SelectedIndex = 5; break;
                case Key.Key7: Inventory.SelectedIndex = 6; break;
                case Key.Key8: Inventory.SelectedIndex = 7; break;
                case Key.Key9: Inventory.SelectedIndex = 8; break;
                case Key.Key0: Inventory.SelectedIndex = 9; break;
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
        float speed = EffectiveSpeed;
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * speed;
            velocity.Z = direction.Z * speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
        }

        Velocity = velocity;
        // GD.Print(Position);
        MoveAndSlide();
    }
}

/// <summary>Lightweight serializable item representation for save files.</summary>
[Serializable]
public class PlrItemData
{
    public string Id { get; set; } = "";
    public int Amount { get; set; }
}

/// <summary>Lightweight serializable buff representation for save files.</summary>
[Serializable]
public class PlrBuffData
{
    public string Id { get; set; } = "";
    public float RemainingTime { get; set; }
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

    // ── Inventory persistence ─────────────────────────────────────
    public PlrItemData[] MainInventory { get; set; } = [];
    public PlrItemData[] CoinSlots { get; set; } = [];
    public PlrItemData[] AmmoSlots { get; set; } = [];
    public int SelectedIndex { get; set; }

    // ── Equipment persistence ─────────────────────────────────────
    public PlrItemData? HeadArmor { get; set; }
    public PlrItemData? BodyArmor { get; set; }
    public PlrItemData? LegsArmor { get; set; }
    public PlrItemData[] Accessories { get; set; } = [];
    public PlrItemData[] VanityArmor { get; set; } = [];
    public PlrItemData[] VanityAccessories { get; set; } = [];
    public PlrItemData? Pet { get; set; }
    public PlrItemData? LightPet { get; set; }
    public PlrItemData? Minecart { get; set; }
    public PlrItemData? Mount { get; set; }
    public PlrItemData? Hook { get; set; }

    // ── Buff persistence ──────────────────────────────────────────
    public PlrBuffData[] ActiveBuffs { get; set; } = [];
}

public class PlrFile
{
    public const string PlrDir = "user://Players";

    /// <summary>Create a new player save file.</summary>
    public void CreatePlayer(string name)
    {
        var plrDir = ProjectSettings.GlobalizePath(PlrDir);
        Directory.CreateDirectory(plrDir);
        var data = new PlrData(name);
        WriteFile(name, data);
    }

    /// <summary>Save the player's current state (including inventory + equipment).</summary>
    public void Save(string name, Player player)
    {
        var data = new PlrData(name)
        {
            Position = player.Position,
            Health = player.Health,
            HealthMax = player.HealthMax,
            Mana = player.Mana,
            ManaMax = player.ManaMax,
            SelectedIndex = player.Inventory.SelectedIndex,
            MainInventory = SerializeSlots(player.Inventory.Main),
            CoinSlots = SerializeSlots(player.Inventory.Coins),
            AmmoSlots = SerializeSlots(player.Inventory.Ammo),
            HeadArmor = SerializeSlot(player.Equipment.HeadArmor),
            BodyArmor = SerializeSlot(player.Equipment.BodyArmor),
            LegsArmor = SerializeSlot(player.Equipment.LegsArmor),
            Accessories = SerializeSlots(player.Equipment.Accessories),
            VanityArmor = SerializeSlots(player.Equipment.VanityArmor),
            VanityAccessories = SerializeSlots(player.Equipment.VanityAccessories),
            Pet = SerializeSlot(player.Equipment.Pet),
            LightPet = SerializeSlot(player.Equipment.LightPet),
            Minecart = SerializeSlot(player.Equipment.Minecart),
            Mount = SerializeSlot(player.Equipment.Mount),
            Hook = SerializeSlot(player.Equipment.Hook),
            ActiveBuffs = SerializeBuffs(player.Buffs),
        };
        WriteFile(name, data);
    }

    public void OpenPlayer(string name, Player player)
    {
        Load(name, player);
    }

    /// <summary>Load player data including inventory and equipment.</summary>
    public void Load(string name, Player player)
    {
        var filePath = GetFilePath(name);
        using var f = File.OpenRead(filePath);
        var data = JsonSerializer.Deserialize<PlrData>(f);
        if (data == null) return;

        player.Position = data.Position;
        player.PlayerName = data.Name;

        // Restore inventory
        DeserializeInto(data.MainInventory, player.Inventory.Main);
        DeserializeInto(data.CoinSlots, player.Inventory.Coins);
        DeserializeInto(data.AmmoSlots, player.Inventory.Ammo);
        player.Inventory.SelectedIndex = data.SelectedIndex;

        // Restore equipment
        DeserializeInto(data.HeadArmor, player.Equipment.HeadArmor);
        DeserializeInto(data.BodyArmor, player.Equipment.BodyArmor);
        DeserializeInto(data.LegsArmor, player.Equipment.LegsArmor);
        DeserializeInto(data.Accessories, player.Equipment.Accessories);
        DeserializeInto(data.VanityArmor, player.Equipment.VanityArmor);
        DeserializeInto(data.VanityAccessories, player.Equipment.VanityAccessories);
        DeserializeInto(data.Pet, player.Equipment.Pet);
        DeserializeInto(data.LightPet, player.Equipment.LightPet);
        DeserializeInto(data.Minecart, player.Equipment.Minecart);
        DeserializeInto(data.Mount, player.Equipment.Mount);
        DeserializeInto(data.Hook, player.Equipment.Hook);

        // Restore buffs
        DeserializeBuffs(data.ActiveBuffs, player.Buffs);
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
        var filePath = GetFilePath(name);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>Check if a player save file exists.</summary>
    public static bool PlayerExists(string name)
    {
        return File.Exists(GetFilePath(name));
    }

    // ── Serialization helpers ─────────────────────────────────────

    private static string GetFilePath(string name)
    {
        return ProjectSettings.GlobalizePath(PlrDir).PathJoin(name + ".plr");
    }

    private static void WriteFile(string name, PlrData data)
    {
        var filePath = GetFilePath(name);
        var json = JsonSerializer.SerializeToUtf8Bytes(data);
        File.WriteAllBytes(filePath, json);
    }

    private static PlrItemData? SerializeSlot(ItemStack? stack)
    {
        if (stack == null || stack.IsEmpty) return null;
        return new PlrItemData { Id = stack.ItemId!, Amount = stack.Amount };
    }

    private static PlrItemData[] SerializeSlots(ItemStack[] slots)
    {
        var result = new List<PlrItemData>();
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
                result.Add(new PlrItemData { Id = slot.ItemId!, Amount = slot.Amount });
        }
        return result.ToArray();
    }

    private static void DeserializeInto(PlrItemData[]? source, ItemStack[] target)
    {
        // Clear all target slots
        foreach (var slot in target) slot.Clear();

        if (source == null) return;

        for (int i = 0; i < Math.Min(source.Length, target.Length); i++)
        {
            if (source[i] != null && !string.IsNullOrEmpty(source[i].Id))
            {
                target[i].ItemId = source[i].Id;
                target[i].Amount = source[i].Amount;
            }
        }
    }

    private static void DeserializeInto(PlrItemData? source, ItemStack target)
    {
        target.Clear();
        if (source != null && !string.IsNullOrEmpty(source.Id))
        {
            target.ItemId = source.Id;
            target.Amount = source.Amount;
        }
    }

    private static PlrBuffData[] SerializeBuffs(BuffManager buffs)
    {
        var state = buffs.GetSerializableState();
        var result = new PlrBuffData[state.Count];
        for (int i = 0; i < state.Count; i++)
        {
            result[i] = new PlrBuffData { Id = state[i].Id, RemainingTime = state[i].Time };
        }
        return result;
    }

    private static void DeserializeBuffs(PlrBuffData[]? source, BuffManager buffs)
    {
        buffs.ClearAll();
        if (source == null) return;

        var state = new List<(string, float)>();
        foreach (var b in source)
        {
            if (!string.IsNullOrEmpty(b.Id))
                state.Add((b.Id, b.RemainingTime));
        }
        buffs.RestoreFromState(state);
    }
}