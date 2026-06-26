using System.Collections.Generic;

namespace Terraria3D.items;

/// <summary>Broad item category for inventory grouping.</summary>
public enum ItemCategory
{
    Material,
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Tool,
    Block,
    Ammo,
    Vanity,
}

/// <summary>
/// Data-driven item definition. Mirrors the same record + registry pattern
/// used by <c>BlockInfo</c> and <c>EntityType</c>.
/// </summary>
public record ItemType(
    string DisplayName,
    ItemCategory Category,
    int MaxStack,
    bool IsConsumable = false,
    string Tooltip = ""
);

/// <summary>
/// Central item registry. One line per item — add new items here.
/// Provides the data backend for inventory slots and world drops.
/// </summary>
public static class ItemRegistry
{
    public static readonly Dictionary<string, ItemType> Types = new()
    {
        ["Gel"] = new(
            DisplayName: "Gel",
            Category: ItemCategory.Material,
            MaxStack: 999
        ),

        ["Wood"] = new(
            DisplayName: "Wood",
            Category: ItemCategory.Block,
            MaxStack: 999
        ),
    };

    /// <summary>Look up item metadata by string ID.</summary>
    public static ItemType Get(string id) => Types[id];

    /// <summary>Check whether an item ID is registered.</summary>
    public static bool Exists(string id) => Types.ContainsKey(id);
}
