using System.Collections.Generic;

namespace Terraria3D.buffs;

/// <summary>Positive or negative effect.</summary>
public enum BuffCategory
{
    Buff,
    Debuff,
}

/// <summary>
/// Stat modifiers a buff can apply. All values default to 0 (no effect).
/// Negative values reduce stats (used by debuffs).
/// </summary>
public record BuffEffect(
    float HealthRegenPerSec = 0f,
    float ManaRegenPerSec = 0f,
    int DefenseBonus = 0,
    float SpeedMultiplier = 0f,
    float DamageMultiplier = 0f,
    int MaxHealthBonus = 0,
    int MaxManaBonus = 0,
    float DamagePerSec = 0f
)
{
    /// <summary>Whether this effect does nothing.</summary>
    public bool IsEmpty =>
        HealthRegenPerSec == 0f
        && ManaRegenPerSec == 0f
        && DefenseBonus == 0
        && SpeedMultiplier == 0f
        && DamageMultiplier == 0f
        && MaxHealthBonus == 0
        && MaxManaBonus == 0
        && DamagePerSec == 0f;

    /// <summary>Combine two effects additively.</summary>
    public static BuffEffect operator +(BuffEffect a, BuffEffect b) => new(
        HealthRegenPerSec: a.HealthRegenPerSec + b.HealthRegenPerSec,
        ManaRegenPerSec: a.ManaRegenPerSec + b.ManaRegenPerSec,
        DefenseBonus: a.DefenseBonus + b.DefenseBonus,
        SpeedMultiplier: a.SpeedMultiplier + b.SpeedMultiplier,
        DamageMultiplier: a.DamageMultiplier + b.DamageMultiplier,
        MaxHealthBonus: a.MaxHealthBonus + b.MaxHealthBonus,
        MaxManaBonus: a.MaxManaBonus + b.MaxManaBonus,
        DamagePerSec: a.DamagePerSec + b.DamagePerSec
    );
}

/// <summary>
/// Data-driven buff definition. One entry per buff type.
/// Follows the same registry pattern as ItemRegistry, BlockRegistry, etc.
/// </summary>
public record BuffData(
    string Name,
    BuffCategory Category,
    float DefaultDuration,     // seconds, 0 = permanent
    BuffEffect? Effects = null,
    string Group = "",
    bool Cancelable = true
);

/// <summary>Central buff registry — one line per buff.</summary>
public static class BuffRegistry
{
    public static readonly Dictionary<string, BuffData> Data = new()
    {
        // ── Potion buffs ──────────────────────────────────────────
        ["Regeneration"] = new(
            Name: "Regeneration",
            Category: BuffCategory.Buff,
            DefaultDuration: 300f,
            Effects: new BuffEffect(HealthRegenPerSec: 2f)
        ),

        ["Swiftness"] = new(
            Name: "Swiftness",
            Category: BuffCategory.Buff,
            DefaultDuration: 240f,
            Effects: new BuffEffect(SpeedMultiplier: 0.25f)
        ),

        ["Ironskin"] = new(
            Name: "Ironskin",
            Category: BuffCategory.Buff,
            DefaultDuration: 240f,
            Effects: new BuffEffect(DefenseBonus: 8)
        ),

        ["ManaRegeneration"] = new(
            Name: "Mana Regeneration",
            Category: BuffCategory.Buff,
            DefaultDuration: 120f,
            Effects: new BuffEffect(ManaRegenPerSec: 2f)
        ),

        // ── Food buffs (same group, replace each other) ───────────
        ["WellFed1"] = new(
            Name: "Well Fed",
            Category: BuffCategory.Buff,
            DefaultDuration: 600f,
            Effects: new BuffEffect(DefenseBonus: 2, SpeedMultiplier: 0.05f),
            Group: "WellFed"
        ),

        ["WellFed2"] = new(
            Name: "Plenty Satisfied",
            Category: BuffCategory.Buff,
            DefaultDuration: 720f,
            Effects: new BuffEffect(DefenseBonus: 3, SpeedMultiplier: 0.075f),
            Group: "WellFed"
        ),

        // ── Debuffs ───────────────────────────────────────────────
        ["Poisoned"] = new(
            Name: "Poisoned",
            Category: BuffCategory.Debuff,
            DefaultDuration: 10f,
            Effects: new BuffEffect(DamagePerSec: 2f),
            Cancelable: false
        ),

        ["OnFire"] = new(
            Name: "On Fire!",
            Category: BuffCategory.Debuff,
            DefaultDuration: 5f,
            Effects: new BuffEffect(DamagePerSec: 4f),
            Cancelable: false
        ),

        ["BrokenArmor"] = new(
            Name: "Broken Armor",
            Category: BuffCategory.Debuff,
            DefaultDuration: 10f,
            Effects: new BuffEffect(DefenseBonus: -20),
            Cancelable: false
        ),

        ["Slow"] = new(
            Name: "Slow",
            Category: BuffCategory.Debuff,
            DefaultDuration: 10f,
            Effects: new BuffEffect(SpeedMultiplier: -0.5f),
            Cancelable: false
        ),

        ["PotionSickness"] = new(
            Name: "Potion Sickness",
            Category: BuffCategory.Debuff,
            DefaultDuration: 60f,
            Cancelable: false
        ),
    };

    /// <summary>Look up buff data by ID.</summary>
    public static BuffData Get(string id) => Data[id];

    /// <summary>Check if a buff ID is registered.</summary>
    public static bool Exists(string id) => Data.ContainsKey(id);
}
