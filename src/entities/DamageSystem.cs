using System;

namespace Terraria3D.entities;

/// <summary>Damage classification. Mirrors classic Terraria damage types.</summary>
public enum DamageType
{
    Melee,
    Ranged,
    Magic,
    Summon,
    Fall,
    Lava,
    Contact,
}

/// <summary>
/// Centralised damage math so every damageable entity uses the same formula.
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// Classic Terraria defence formula: net damage = max(1, raw - defence * 0.5),
    /// then scaled by knockback resistance to also reduce damage (resistance
    /// pulls double-duty as damage reduction).
    /// </summary>
    // type reserved for future damage-type-specific modifiers (e.g. fire vs ice)
    public static int Calculate(int baseDamage, DamageType _type, int defense, float resistance)
    {
        float defenseFactor = defense * 0.5f;
        int netDamage = Math.Max(1, (int)(baseDamage - defenseFactor));
        return (int)(netDamage * (1.0f - resistance));
    }

    /// <summary>Knockback force after resistance.</summary>
    public static float Knockback(float baseForce, float resistance)
    {
        return baseForce * (1.0f - resistance);
    }
}
