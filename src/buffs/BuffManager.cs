using System.Collections.Generic;
using Godot;

namespace Terraria3D.buffs;

/// <summary>An active buff instance on a player, with remaining duration.</summary>
public class ActiveBuff
{
    public string BuffId { get; set; } = "";
    public float RemainingTime { get; set; }

    public BuffData Data => BuffRegistry.Get(BuffId);
    public bool IsExpired => RemainingTime < 0f;
    public bool IsPermanent => RemainingTime == 0f;

    public ActiveBuff(string buffId, float duration)
    {
        BuffId = buffId;
        RemainingTime = duration;
    }
}

/// <summary>
/// Per-player buff manager. Tracks active buffs, ticks durations,
/// applies regeneration and damage-over-time, and computes combined
/// stat modifiers from all active buffs.
/// </summary>
public class BuffManager
{
    private readonly List<ActiveBuff> _buffs = [];

    /// <summary>Read-only view of all active buffs.</summary>
    public IReadOnlyList<ActiveBuff> ActiveBuffs => _buffs;

    /// <summary>
    /// Process all active buffs for one frame: tick durations, expire
    /// elapsed buffs, apply regeneration and damage-over-time to the player.
    /// </summary>
    public void Process(double delta, Player player)
    {
        float dt = (float)delta;
        var combined = GetCombinedEffects();

        // Apply regeneration
        if (combined.HealthRegenPerSec != 0f)
            player.Health = Mathf.Min(player.EffectiveMaxHealth, player.Health + (int)(combined.HealthRegenPerSec * dt));

        if (combined.ManaRegenPerSec != 0f)
            player.Mana = Mathf.Min(player.EffectiveMaxMana, player.Mana + (int)(combined.ManaRegenPerSec * dt));

        // Apply damage-over-time (debuffs)
        if (combined.DamagePerSec > 0f)
            player.Health -= (int)(combined.DamagePerSec * dt);

        // Tick durations and expire
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            if (_buffs[i].IsPermanent) continue;

            _buffs[i].RemainingTime -= dt;
            if (_buffs[i].IsExpired)
            {
                GD.Print($"[Buff] {_buffs[i].BuffId} expired");
                _buffs.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Apply or refresh a buff. If a buff with the same Group is already
    /// active, the old one is replaced. If the same buff ID is already
    /// active, its duration is refreshed (or extended).
    /// </summary>
    public void ApplyBuff(string buffId, float? customDuration = null)
    {
        if (!BuffRegistry.Exists(buffId)) return;

        var data = BuffRegistry.Get(buffId);
        float duration = customDuration ?? data.DefaultDuration;

        // Same-group replacement
        if (!string.IsNullOrEmpty(data.Group))
        {
            _buffs.RemoveAll(b => BuffRegistry.Exists(b.BuffId) && BuffRegistry.Get(b.BuffId).Group == data.Group);
        }

        // Refresh existing same-ID buff
        var existing = _buffs.Find(b => b.BuffId == buffId);
        if (existing != null)
        {
            existing.RemainingTime = Mathf.Max(existing.RemainingTime, duration);
            GD.Print($"[Buff] Refreshed {buffId} to {existing.RemainingTime}s");
            return;
        }

        _buffs.Add(new ActiveBuff(buffId, duration));
        GD.Print($"[Buff] Applied {buffId} for {duration}s");
    }

    /// <summary>Remove a buff by ID (e.g., right-click cancel, equipment unequip).</summary>
    public void RemoveBuff(string buffId)
    {
        _buffs.RemoveAll(b => b.BuffId == buffId);
    }

    /// <summary>Check if a buff is currently active.</summary>
    public bool HasBuff(string buffId)
    {
        return _buffs.Exists(b => b.BuffId == buffId);
    }

    /// <summary>
    /// Compute the combined stat effects from all active buffs.
    /// Buffs stack additively using <see cref="BuffEffect.operator+"/>.
    /// </summary>
    public BuffEffect GetCombinedEffects()
    {
        BuffEffect total = new();
        foreach (var b in _buffs)
        {
            if (b.Data.Effects != null)
                total += b.Data.Effects;
        }
        return total;
    }

    /// <summary>Remove all active buffs (e.g., on death).</summary>
    public void ClearAll()
    {
        _buffs.Clear();
    }

    /// <summary>Get active buff IDs and their remaining times for serialization.</summary>
    public List<(string Id, float Time)> GetSerializableState()
    {
        var state = new List<(string, float)>();
        foreach (var b in _buffs)
            state.Add((b.BuffId, b.RemainingTime));
        return state;
    }

    /// <summary>Restore buffs from serialized state.</summary>
    public void RestoreFromState(List<(string Id, float Time)> state)
    {
        _buffs.Clear();
        foreach (var (id, time) in state)
        {
            if (BuffRegistry.Exists(id))
                _buffs.Add(new ActiveBuff(id, time));
        }
    }
}
