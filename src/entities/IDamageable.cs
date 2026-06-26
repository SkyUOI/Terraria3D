using Godot;

namespace Terraria3D.entities;

/// <summary>
/// Interface for anything that can receive damage — the Player, enemies,
/// NPCs, and bosses. The damage system operates on this interface so it
/// never needs to know the concrete type.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount, Vector3 knockbackDirection, float knockbackForce, DamageType type);
    Vector3 GlobalPosition { get; }
    bool IsDead { get; }
}
