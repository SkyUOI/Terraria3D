using System.Collections.Generic;
using Godot;

namespace Terraria3D.entities.components;

/// <summary>One entry in a drop table.</summary>
public record LootEntry(
    string ItemId,
    float DropChance,
    int MinAmount = 1,
    int MaxAmount = 1
);

/// <summary>
/// Drop table attached to an entity. When the entity dies, <c>Roll</c>
/// returns the list of items that should be spawned.
/// </summary>
public partial class LootComponent : Node
{
    /// <summary>Drop table. Set programmatically or via JSON data.</summary>
    public LootEntry[] LootTable { get; set; } = [];

    /// <summary>
    /// Default loot table for known entity types. Called by SpawnManager
    /// to populate the loot table when an entity is spawned.
    /// </summary>
    public static LootEntry[] GetDefaultTable(string entityId) => entityId switch
    {
        "GreenSlime" => [new LootEntry("Gel", 1.0f, 1, 3)],
        "BlueSlime"  => [new LootEntry("Gel", 1.0f, 2, 5)],
        "DemonEye"   => [new LootEntry("Gel", 0.5f, 1, 2)],
        _ => []
    };

    /// <summary>
    /// Roll the drop table and return the IDs of items that dropped.
    /// Each entry is rolled independently.
    /// </summary>
    public List<(string ItemId, int Amount)> Roll()
    {
        var drops = new List<(string, int)>();
        foreach (var entry in LootTable)
        {
            if (GD.Randf() < entry.DropChance)
            {
                int amount = entry.MinAmount == entry.MaxAmount
                    ? entry.MinAmount
                    : GD.RandRange(entry.MinAmount, entry.MaxAmount);
                drops.Add((entry.ItemId, amount));
            }
        }
        return drops;
    }
}
