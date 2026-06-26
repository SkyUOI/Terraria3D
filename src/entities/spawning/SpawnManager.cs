using System.Collections.Generic;
using Godot;
using Terraria3D.entities.components;

namespace Terraria3D.entities.spawning;

/// <summary>
/// Manages enemy spawning around the player. Uses a Timer child node to
/// drive periodic spawn attempts. Timer properties and signal connections
/// are configured in the editor.
/// </summary>
public partial class SpawnManager : Node3D
{
    /// <summary>Timer child that triggers spawn attempts.</summary>
    [Export]
    public Timer SpawnTimer { get; set; }

    /// <summary>Seconds between spawn attempts. Reads from / writes to the Timer node.</summary>
    [Export]
    public double SpawnInterval
    {
        get => SpawnTimer.WaitTime;
        set { SpawnTimer.WaitTime = value; }
    }

    /// <summary>Maximum number of active spawned entities at once.</summary>
    [Export]
    public int MaxSpawns { get; set; } = 10;

    /// <summary>Horizontal spawn range in world units.</summary>
    [Export]
    public float SpawnHorizontalRange { get; set; } = 42f;

    /// <summary>Vertical spawn range in world units.</summary>
    [Export]
    public float SpawnVerticalRange { get; set; } = 23.5f;

    /// <summary>No-spawn radius around the player in world units.</summary>
    [Export]
    public float SafeZoneRadius { get; set; } = 31f;

    /// <summary>Entities farther than this despawn in world units.</summary>
    [Export]
    public float DespawnRange { get; set; } = 60f;

    /// <summary>Max spawn attempts per interval before giving up.</summary>
    [Export]
    public int MaxSpawnAttempts { get; set; } = 30;

    /// <summary>Reference to the player for distance checks.</summary>
    [Export]
    public Node3D Target { get; set; }

    /// <summary>Reference to Main for world state (time, day/night).</summary>
    [Export]
    public Main Main { get; set; }

    /// <summary>Reference to the VoxelTerrain for biome block counting.</summary>
    [Export]
    public VoxelTerrain Terrain { get; set; }

    /// <summary>Scene template for dropped items assigned to spawned entities.</summary>
    [Export]
    public PackedScene DroppedItemScene { get; set; }

    /// <summary>Reference to the Player (not just Node3D) for item pickup wiring.</summary>
    [Export]
    public Player Player { get; set; }

    /// <summary>Parent node under which entities are spawned.</summary>
    [Export]
    public Node EntitiesContainer { get; set; }

    private readonly List<Entity> _activeEntities = [];

    private void OnSpawnTimerTimeout()
    {
        CleanupFarEntities();
        CleanupDeadEntities();

        if (_activeEntities.Count < MaxSpawns)
            AttemptSpawn();
    }

    private void CleanupFarEntities()
    {
        if (Target == null) return;

        for (int i = _activeEntities.Count - 1; i >= 0; i--)
        {
            var entity = _activeEntities[i];
            if (!IsInstanceValid(entity)) continue;

            float dist = entity.GlobalPosition.DistanceTo(Target.GlobalPosition);
            if (dist > DespawnRange)
            {
                GD.Print($"[SpawnManager] Despawning {entity.TypeId} (distance: {dist:F1})");
                _activeEntities.RemoveAt(i);
                entity.QueueFree();
            }
        }
    }

    private void CleanupDeadEntities()
    {
        _activeEntities.RemoveAll(e => !IsInstanceValid(e) || e.IsDead);
    }

    private void AttemptSpawn()
    {
        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            Vector3 spawnPos = PickSpawnPosition();
            if (!IsValidSpawnPosition(spawnPos)) continue;

            // Convert world position to voxel coordinates
            // VoxelTerrain scale = 0.1, BlockSize = 0.5 → voxel = world / 0.05
            float scale = Constants.BlockSize * 0.1f;
            var voxelCenter = new Vector3I(
                (int)(Target.GlobalPosition.X / scale),
                (int)(Target.GlobalPosition.Y / scale),
                (int)(Target.GlobalPosition.Z / scale)
            );

            // Block-counting biome detection (Terraria-style)
            var voxelTool = Terrain.GetVoxelTool();
            voxelTool.Channel = (int)VoxelBuffer.ChannelId.ChannelType;
            var biome = BiomeDetector.GetBiome(voxelCenter, voxelTool);

            // Depth layer from noise-based surface height (cheap approximation)
            int surfaceHeight = BiomeDetector.GetSurfaceHeight(
                Target.GlobalPosition.X, Target.GlobalPosition.Z);
            var depthLayer = BiomeDetector.GetDepthLayer(voxelCenter.Y, surfaceHeight);

            var pool = SpawnTables.GetPool(
                biome.ToString(), Main.IsNight, (int)depthLayer);
            string typeId = SpawnTables.PickRandom(pool);

            if (!CanSpawnType(typeId)) continue;

            SpawnEntity(typeId, spawnPos);
            break;
        }
    }

    private Vector3 PickSpawnPosition()
    {
        float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
        float dist = SafeZoneRadius + (float)GD.RandRange(0, SpawnHorizontalRange - SafeZoneRadius);
        float x = Target.GlobalPosition.X + Mathf.Cos(angle) * dist;
        float z = Target.GlobalPosition.Z + Mathf.Sin(angle) * dist;

        float yOffset = (float)GD.RandRange(-SpawnVerticalRange, SpawnVerticalRange);
        float y = Target.GlobalPosition.Y + yOffset;

        return new Vector3(x, y, z);
    }

    private bool IsValidSpawnPosition(Vector3 pos)
    {
        if (Target == null) return false;

        if (pos.DistanceTo(Target.GlobalPosition) < SafeZoneRadius)
            return false;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = new PhysicsRayQueryParameters3D
        {
            From = pos + Vector3.Up * 10f,
            To = pos + Vector3.Down * 10f,
            CollisionMask = 1,
        };
        var result = spaceState.IntersectRay(query);

        return result.Count > 0;
    }

    private bool CanSpawnType(string typeId)
    {
        if (!EntityRegistry.Exists(typeId)) return false;

        var data = EntityRegistry.Get(typeId);
        int count = 0;
        foreach (var entity in _activeEntities)
        {
            if (IsInstanceValid(entity) && entity.TypeId == typeId)
                count++;
        }
        return count < data.MaxSpawnCount;
    }

    private void SpawnEntity(string typeId, Vector3 position)
    {
        string scenePath = $"res://src/entities/enemies/{typeId}.tscn";
        PackedScene scene;
        try
        {
            scene = GD.Load<PackedScene>(scenePath);
        }
        catch
        {
            GD.PushError($"[SpawnManager] Cannot load scene: {scenePath}");
            return;
        }

        var instance = scene.Instantiate<Entity>();
        instance.TypeId = typeId;
        instance.GlobalPosition = position;
        instance.GlobalRotation = new Vector3(0, (float)GD.RandRange(0, Mathf.Pi * 2), 0);

        // Wire drop system
        instance.DroppedItemScene = DroppedItemScene;
        instance.Player = Player;

        // Configure default loot table
        if (instance.Loot != null)
            instance.Loot.LootTable = LootComponent.GetDefaultTable(typeId);

        EntitiesContainer.AddChild(instance);
        _activeEntities.Add(instance);

        GD.Print($"[SpawnManager] Spawned {typeId} at {position}");
    }
}
