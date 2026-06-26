using System.Collections.Generic;

namespace Terraria3D.entities;

/// <summary>Broad entity classification used by spawn tables and save data.</summary>
public enum EntityCategory
{
    Enemy,
    Npc,
    Boss,
    Critter,
}

/// <summary>
/// Data-driven entity definition — one record per entity type. The runtime
/// creates entities from these definitions rather than requiring a unique C#
/// class for each mob. Mirrors the <c>BlockInfo</c> pattern in Blocks.cs.
/// </summary>
public record EntityType(
    string DisplayName,
    EntityCategory Category,
    int BaseHealth,
    int Defense,
    float MoveSpeed,
    float KnockbackResistance,
    bool IsFlying,
    int ContactDamage,
    float AggroRange,
    string[] SpawnBiomes,
    int MinSpawnDepth,
    int MaxSpawnDepth,
    int MaxSpawnCount
);

/// <summary>
/// Single entry point for entity type queries. One line per entity type —
/// add new entries here when adding new mobs. The string key is the internal
/// ID used everywhere (scene filenames, save data, spawn tables).
/// </summary>
public static class EntityRegistry
{
    public static readonly Dictionary<string, EntityType> Types = new()
    {
        ["GreenSlime"] = new(
            DisplayName: "Green Slime",
            Category: EntityCategory.Enemy,
            BaseHealth: 50,
            Defense: 2,
            MoveSpeed: 1.5f,
            KnockbackResistance: 0.3f,
            IsFlying: false,
            ContactDamage: 7,
            AggroRange: 12.0f,
            SpawnBiomes: ["Forest"],
            MinSpawnDepth: 0,
            MaxSpawnDepth: 200,
            MaxSpawnCount: 5
        ),

        ["BlueSlime"] = new(
            DisplayName: "Blue Slime",
            Category: EntityCategory.Enemy,
            BaseHealth: 75,
            Defense: 4,
            MoveSpeed: 1.7f,
            KnockbackResistance: 0.4f,
            IsFlying: false,
            ContactDamage: 10,
            AggroRange: 14.0f,
            SpawnBiomes: ["Forest"],
            MinSpawnDepth: 0,
            MaxSpawnDepth: 200,
            MaxSpawnCount: 3
        ),

        ["DemonEye"] = new(
            DisplayName: "Demon Eye",
            Category: EntityCategory.Enemy,
            BaseHealth: 60,
            Defense: 2,
            MoveSpeed: 3.0f,
            KnockbackResistance: 0.2f,
            IsFlying: true,
            ContactDamage: 8,
            AggroRange: 20.0f,
            SpawnBiomes: ["Forest"],
            MinSpawnDepth: 0,
            MaxSpawnDepth: 200,
            MaxSpawnCount: 3
        ),
    };

    /// <summary>Look up entity metadata by its string ID.</summary>
    public static EntityType Get(string id) => Types[id];

    /// <summary>Check whether a type ID is registered.</summary>
    public static bool Exists(string id) => Types.ContainsKey(id);
}
