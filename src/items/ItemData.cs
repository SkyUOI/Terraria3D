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
    string Tooltip = "",
    int Defense = 0,
    float DamageBonus = 0f,
    float SpeedBonus = 0f,
    int ManaBonus = 0,
    int HealthBonus = 0
);

/// <summary>
/// Central item registry. One line per item — add new items here.
/// Provides the data backend for inventory slots and world drops.
/// </summary>
public static class ItemRegistry
{
    public static readonly Dictionary<string, ItemType> Types = new()
    {
        // ── Materials ────────────────────────────────────────────
        ["Gel"] = new(
            DisplayName: "Gel",
            Category: ItemCategory.Material,
            MaxStack: 999
        ),

        // ── Blocks ───────────────────────────────────────────────
        ["Wood"] = new(
            DisplayName: "Wood",
            Category: ItemCategory.Block,
            MaxStack: 999
        ),

        ["DirtBlock"] = new(
            DisplayName: "Dirt Block",
            Category: ItemCategory.Block,
            MaxStack: 999
        ),

        ["StoneBlock"] = new(
            DisplayName: "Stone Block",
            Category: ItemCategory.Block,
            MaxStack: 999
        ),

        ["Torch"] = new(
            DisplayName: "Torch",
            Category: ItemCategory.Block,
            MaxStack: 99,
            Tooltip: "Provides light when placed or held"
        ),

        // ── Armor (Wood set) ─────────────────────────────────────
        ["WoodHelmet"] = new(
            DisplayName: "Wood Helmet",
            Category: ItemCategory.Armor,
            MaxStack: 1,
            Defense: 1,
            Tooltip: "Part of the Wood armor set"
        ),

        ["WoodBreastplate"] = new(
            DisplayName: "Wood Breastplate",
            Category: ItemCategory.Armor,
            MaxStack: 1,
            Defense: 1,
            Tooltip: "Part of the Wood armor set"
        ),

        ["WoodGreaves"] = new(
            DisplayName: "Wood Greaves",
            Category: ItemCategory.Armor,
            MaxStack: 1,
            Defense: 0,
            Tooltip: "Part of the Wood armor set"
        ),

        // ── Weapons ──────────────────────────────────────────────
        ["WoodenSword"] = new(
            DisplayName: "Wooden Sword",
            Category: ItemCategory.Weapon,
            MaxStack: 1,
            Tooltip: "A simple wooden blade"
        ),

        ["CopperShortsword"] = new(
            DisplayName: "Copper Shortsword",
            Category: ItemCategory.Weapon,
            MaxStack: 1,
            Tooltip: "A basic copper sword"
        ),

        // ── Tools ────────────────────────────────────────────────
        ["CopperPickaxe"] = new(
            DisplayName: "Copper Pickaxe",
            Category: ItemCategory.Tool,
            MaxStack: 1,
            Tooltip: "Can mine stone and basic ores"
        ),

        ["CopperAxe"] = new(
            DisplayName: "Copper Axe",
            Category: ItemCategory.Tool,
            MaxStack: 1,
            Tooltip: "Can chop down trees"
        ),

        // ── Accessories ──────────────────────────────────────────
        ["HermesBoots"] = new(
            DisplayName: "Hermes Boots",
            Category: ItemCategory.Accessory,
            MaxStack: 1,
            SpeedBonus: 0.3f,
            Tooltip: "The wearer can run super fast"
        ),

        ["BandOfRegeneration"] = new(
            DisplayName: "Band of Regeneration",
            Category: ItemCategory.Accessory,
            MaxStack: 1,
            Tooltip: "Slowly regenerates life"
        ),

        ["CloudInABottle"] = new(
            DisplayName: "Cloud in a Bottle",
            Category: ItemCategory.Accessory,
            MaxStack: 1,
            Tooltip: "Allows the holder to double jump"
        ),

        // ── Consumables ──────────────────────────────────────────
        ["LesserHealingPotion"] = new(
            DisplayName: "Lesser Healing Potion",
            Category: ItemCategory.Consumable,
            MaxStack: 30,
            IsConsumable: true,
            Tooltip: "Restores 50 health"
        ),

        ["LesserManaPotion"] = new(
            DisplayName: "Lesser Mana Potion",
            Category: ItemCategory.Consumable,
            MaxStack: 30,
            IsConsumable: true,
            Tooltip: "Restores 50 mana"
        ),

        // ── Ammo ─────────────────────────────────────────────────
        ["WoodenArrow"] = new(
            DisplayName: "Wooden Arrow",
            Category: ItemCategory.Ammo,
            MaxStack: 999,
            Tooltip: "Basic ammunition for bows"
        ),
    };

    /// <summary>Look up item metadata by string ID.</summary>
    public static ItemType Get(string id) => Types[id];

    /// <summary>Check whether an item ID is registered.</summary>
    public static bool Exists(string id) => Types.ContainsKey(id);
}
