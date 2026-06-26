using Godot;

namespace Terraria3D.entities.bosses;

/// <summary>One phase in a boss fight.</summary>
public record BossPhase(
    string Name,
    float HealthThreshold,  // fraction of max HP (0..1) at which this phase activates
    float MoveSpeedMultiplier,
    int ContactDamageMultiplier
);

/// <summary>
/// Boss base class. Adds multi-phase AI and boss-specific UI hooks.
/// Bosses are spawned via summoning items, not the regular spawn system.
/// </summary>
public partial class Boss : Entity
{
    /// <summary>Boss phases. Set programmatically or via JSON data.</summary>
    public BossPhase[] Phases { get; set; } = [];

    /// <summary>Index into Phases for the currently active phase.</summary>
    public int CurrentPhaseIndex { get; protected set; }

    /// <summary>Current active phase data (read-only).</summary>
    public BossPhase CurrentPhase => Phases[CurrentPhaseIndex];

    /// <summary>Emitted when the boss changes phase.</summary>
    [Signal]
    public delegate void PhaseChangedEventHandler(int newPhaseIndex, string phaseName);

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("boss");

        // Sort phases by threshold descending so we match highest threshold first
        System.Array.Sort(Phases, (a, b) => b.HealthThreshold.CompareTo(a.HealthThreshold));

        GD.Print($"[Boss] {TypeId} spawned with {Phases.Length} phases");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (IsDead) return;

        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        if (Health == null || Phases.Length == 0) return;

        float hpFraction = (float)Health.CurrentHealth / Health.MaxHealth;

        for (int i = CurrentPhaseIndex + 1; i < Phases.Length; i++)
        {
            if (hpFraction <= Phases[i].HealthThreshold)
            {
                CurrentPhaseIndex = i;
                OnPhaseChanged(i);
                EmitSignal(SignalName.PhaseChanged, i, Phases[i].Name);
                break;
            }
        }
    }

    /// <summary>Called when the boss enters a new phase. Override for custom logic.</summary>
    protected virtual void OnPhaseChanged(int phaseIndex)
    {
        var phase = Phases[phaseIndex];
        MoveSpeed *= phase.MoveSpeedMultiplier;
        ContactDamage *= phase.ContactDamageMultiplier;

        GD.Print($"[Boss] {TypeId} entered phase {phaseIndex}: {phase.Name}");
    }

    public override void OnDeath()
    {
        GD.Print($"[Boss] {TypeId} defeated!");
        // Future: mark boss as defeated in world data, drop guaranteed loot
        base.OnDeath();
    }
}
