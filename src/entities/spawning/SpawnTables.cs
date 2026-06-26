using System.Collections.Generic;
using Godot;

namespace Terraria3D.entities.spawning;

/// <summary>Weighted entry in a spawn pool.</summary>
public record SpawnEntry(string EntityId, float Weight);

/// <summary>A collection of spawn entries that form a pool for a biome/time.</summary>
public record SpawnPool(List<SpawnEntry> Entries)
{
    public SpawnPool(params (string EntityId, float Weight)[] entries)
        : this(new List<SpawnEntry>())
    {
        foreach (var (id, weight) in entries)
            Entries.Add(new SpawnEntry(id, weight));
    }
}

/// <summary>
/// Spawn pools keyed by biome + time-of-day. Each pool is a weighted list
/// of entity type IDs. The spawn manager picks a random entity from the
/// active pool when spawning.
/// </summary>
public static class SpawnTables
{
    public static readonly Dictionary<string, SpawnPool> Pools = new()
    {
        ["ForestDay"] = new SpawnPool(
            ("GreenSlime", 0.5f),
            ("BlueSlime", 0.3f)
        ),

        ["ForestNight"] = new SpawnPool(
            ("DemonEye", 0.4f),
            ("GreenSlime", 0.3f),
            ("BlueSlime", 0.3f)
        ),

        ["DesertDay"] = new SpawnPool(
            ("GreenSlime", 0.5f),
            ("BlueSlime", 0.3f)
        ),

        ["DesertNight"] = new SpawnPool(
            ("DemonEye", 0.4f),
            ("GreenSlime", 0.3f),
            ("BlueSlime", 0.3f)
        ),

        ["SnowDay"] = new SpawnPool(
            ("BlueSlime", 0.5f),
            ("GreenSlime", 0.3f)
        ),

        ["SnowNight"] = new SpawnPool(
            ("DemonEye", 0.4f),
            ("BlueSlime", 0.4f)
        ),

        ["JungleDay"] = new SpawnPool(
            ("GreenSlime", 0.6f),
            ("BlueSlime", 0.2f)
        ),

        ["JungleNight"] = new SpawnPool(
            ("DemonEye", 0.3f),
            ("GreenSlime", 0.4f),
            ("BlueSlime", 0.3f)
        ),

        // Default fallback used when no specific pool matches
        ["Default"] = new SpawnPool(
            ("GreenSlime", 1.0f)
        ),
    };

    /// <summary>
    /// Returns the spawn pool for the current biome + time combination.
    /// Falls back to "Default" if no specific pool is defined.
    /// </summary>
    public static SpawnPool GetPool(string biome, bool isNight, int depth)
    {
        // Future: check depth for cavern pool
        string key = biome + (isNight ? "Night" : "Day");
        return Pools.GetValueOrDefault(key, Pools["Default"]);
    }

    /// <summary>
    /// Pick a random entity ID from the pool, weighted by each entry's weight.
    /// </summary>
    public static string PickRandom(SpawnPool pool)
    {
        float totalWeight = 0f;
        foreach (var entry in pool.Entries)
            totalWeight += entry.Weight;

        float roll = (float)GD.RandRange(0, totalWeight);
        float cumulative = 0f;
        foreach (var entry in pool.Entries)
        {
            cumulative += entry.Weight;
            if (roll <= cumulative)
                return entry.EntityId;
        }

        // Fallback: return first entry
        return pool.Entries[0].EntityId;
    }
}
