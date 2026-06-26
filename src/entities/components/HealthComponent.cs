using Godot;

namespace Terraria3D.entities.components;

/// <summary>
/// Reusable health container attached to any <see cref="IDamageable"/> entity.
/// Handles damage intake, invincibility frames, and death signalling.
/// Add as a child Node to any entity that needs health.
/// </summary>
public partial class HealthComponent : Node
{
    /// <summary>Duration in seconds the entity is invulnerable after taking damage.</summary>
    public const float InvincibilityDuration = 0.66f;

    [Export]
    public int MaxHealth { get; set; } = 100;

    [Export]
    public int Defense { get; set; } = 0;

    [Export]
    public float KnockbackResistance { get; set; } = 0f;

    /// <summary>Current health. Starts at MaxHealth on _Ready.</summary>
    public int CurrentHealth { get; set; }

    /// <summary>True while the entity cannot take damage.</summary>
    public bool IsInvincible { get; private set; }

    /// <summary>True after CurrentHealth reaches 0.</summary>
    public bool IsDead { get; private set; }

    private float _invincibilityTimer;

    /// <summary>Emitted when the entity dies. Connect in the editor or code.</summary>
    [Signal]
    public delegate void DiedEventHandler();

    /// <summary>Emitted when the entity takes damage (after invincibility check).</summary>
    [Signal]
    public delegate void DamagedEventHandler(int damage, Vector3 knockbackDir, float knockbackForce);

    public override void _Ready()
    {
        base._Ready();
        CurrentHealth = MaxHealth;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (IsInvincible)
        {
            _invincibilityTimer -= (float)delta;
            if (_invincibilityTimer <= 0f)
            {
                IsInvincible = false;
                _invincibilityTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Apply damage with knockback. Respects invincibility frames and defence.
    /// Returns true if damage was actually applied.
    /// </summary>
    public bool TakeDamage(int amount, DamageType type, Vector3 knockbackDir, float knockbackForce)
    {
        if (IsInvincible || IsDead)
            return false;

        int actualDamage = DamageCalculator.Calculate(amount, type, Defense, KnockbackResistance);
        float actualKnockback = DamageCalculator.Knockback(knockbackForce, KnockbackResistance);

        CurrentHealth = Mathf.Max(0, CurrentHealth - actualDamage);
        IsInvincible = true;
        _invincibilityTimer = InvincibilityDuration;

        EmitSignal(SignalName.Damaged, actualDamage, knockbackDir, actualKnockback);

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            EmitSignal(SignalName.Died);
        }

        return true;
    }

    /// <summary>Heal by the given amount, clamped to MaxHealth.</summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }
}
