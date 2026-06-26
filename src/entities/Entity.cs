using Godot;
using Terraria3D.entities.components;
using Terraria3D.items;

namespace Terraria3D.entities;

/// <summary>
/// Abstract base class for all living game entities (enemies, NPCs, bosses).
/// Provides shared physics (CharacterBody3D), health, AI, and loot plumbing.
/// Child node references are exported and set in the editor.
/// </summary>
public abstract partial class Entity : CharacterBody3D, IDamageable
{
    /// <summary>Internal type ID used to look up EntityRegistry data.</summary>
    [Export]
    public string TypeId { get; set; } = string.Empty;

    // --- Child node references (set in editor) ---

    [Export]
    public CollisionShape3D HitCollider { get; set; }

    [Export]
    public MeshInstance3D BodyMesh { get; set; }

    [Export]
    public HealthComponent Health { get; set; }

    [Export]
    public AiController Ai { get; set; }

    [Export]
    public LootComponent Loot { get; set; }

    /// <summary>Scene template for dropped items (set by SpawnManager).</summary>
    [Export]
    public PackedScene DroppedItemScene { get; set; }

    /// <summary>Reference to the player (set by SpawnManager) for item pickup.</summary>
    [Export]
    public Player Player { get; set; }

    // --- Exported stats (overridden by EntityRegistry on spawn) ---
    [Export]
    public float MoveSpeed { get; set; } = 2.0f;

    [Export]
    public int ContactDamage { get; set; } = 5;

    [Export]
    public float AggroRange { get; set; } = 12.0f;

    [Export]
    public bool IsFlying { get; set; }

    /// <summary>Is this entity dead (health ≤ 0)?</summary>
    public bool IsDead => Health != null && Health.IsDead;

    public override void _Ready()
    {
        base._Ready();

        // Apply data from EntityRegistry if TypeId is set
        if (!string.IsNullOrEmpty(TypeId) && EntityRegistry.Exists(TypeId))
        {
            var data = EntityRegistry.Get(TypeId);
            if (Health != null)
            {
                Health.MaxHealth = data.BaseHealth;
                Health.Defense = data.Defense;
                Health.KnockbackResistance = data.KnockbackResistance;
                Health.CurrentHealth = data.BaseHealth;
            }
            MoveSpeed = data.MoveSpeed;
            ContactDamage = data.ContactDamage;
            AggroRange = data.AggroRange;
            IsFlying = data.IsFlying;
        }

        // Propagate config to AI controller
        if (Ai != null)
        {
            Ai.AggroRange = AggroRange;
            Ai.Entity = this;
        }

        // Connect health signals
        if (Health != null)
        {
            Health.Died += OnDied;
        }

        GD.Print($"[Entity] {TypeId} spawned at {GlobalPosition}");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (IsDead) return;

        // Apply gravity unless flying
        if (!IsFlying && !IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
        }
        else if (IsFlying)
        {
            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
        }

        MoveAndSlide();
    }

    // --- IDamageable implementation ---

    public void TakeDamage(int amount, Vector3 knockbackDirection, float knockbackForce, DamageType type)
    {
        if (Health == null || IsDead) return;

        bool applied = Health.TakeDamage(amount, type, knockbackDirection, knockbackForce);
        if (applied)
        {
            Velocity += knockbackDirection * knockbackForce;
            OnDamage(amount, knockbackDirection, knockbackForce);
        }
    }

    // --- Movement helpers called by AiController ---

    /// <summary>Move in a normalised direction at MoveSpeed.</summary>
    public void MoveInDirection(Vector3 direction, float delta)
    {
        var vel = Velocity;
        vel.X = direction.X * MoveSpeed;
        vel.Z = direction.Z * MoveSpeed;
        Velocity = vel;
    }

    /// <summary>Steer toward a world-space point.</summary>
    public void MoveToward(Vector3 target, float delta)
    {
        Vector3 toTarget = target - GlobalPosition;
        toTarget.Y = 0;
        if (toTarget.Length() < 0.5f)
        {
            Velocity = new Vector3(0, Velocity.Y, 0);
            return;
        }
        MoveInDirection(toTarget.Normalized(), delta);
    }

    /// <summary>Stop horizontal movement.</summary>
    public void StopMoving()
    {
        Velocity = new Vector3(0, Velocity.Y, 0);
    }

    // --- Virtual callbacks for subclasses ---

    public virtual void PerformAttack(IDamageable target, double delta)
    {
    }

    public virtual void OnDamage(int amount, Vector3 knockbackDir, float knockbackForce)
    {
        GD.Print($"[Entity] {TypeId} took {amount} damage, HP: {Health?.CurrentHealth}");
    }

    public virtual void OnDeath()
    {
        GD.Print($"[Entity] {TypeId} died");

        if (Loot != null && DroppedItemScene != null && Player != null)
        {
            var drops = Loot.Roll();
            foreach (var (itemId, amount) in drops)
            {
                if (!ItemRegistry.Exists(itemId)) continue;

                var item = DroppedItemScene.Instantiate<DroppedItem>();
                item.ItemId = itemId;
                item.Amount = amount;
                item.Target = Player;
                item.GlobalPosition = GlobalPosition;
                GetParent().AddChild(item);
                GD.Print($"[Entity] {TypeId} dropped {amount}x {itemId}");
            }
        }

        QueueFree();
    }

    private void OnDied()
    {
        OnDeath();
    }
}
