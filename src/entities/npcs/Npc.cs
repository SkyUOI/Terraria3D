using Godot;

namespace Terraria3D.entities.npcs;

/// <summary>
/// Friendly town NPC base class. NPCs are permanent world residents that
/// provide services (shops, healing, quests) and are saved with the world.
/// </summary>
public partial class Npc : Entity
{
    /// <summary>Unique NPC ID for save/load (e.g. "Guide", "Merchant").</summary>
    [Export]
    public string NpcId { get; set; } = string.Empty;

    /// <summary>Whether this NPC has moved into the world permanently.</summary>
    public bool HasMovedIn { get; set; }

    /// <summary>Where the NPC returns when idle.</summary>
    public Vector3 HomePosition { get; set; }

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("npc");
    }

    public override void OnDeath()
    {
        // Town NPCs don't permanently die — they respawn later
        GD.Print($"[NPC] {NpcId} has been slain (will respawn)");
        // Future: schedule respawn after a delay
        QueueFree();
    }
}
