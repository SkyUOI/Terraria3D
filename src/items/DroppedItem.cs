using Godot;
using Terraria3D;

namespace Terraria3D.items;

/// <summary>
/// A physical item dropped into the 3D world. Falls with gravity, bounces
/// on spawn, and is picked up when the player walks near it.
/// Auto-despawns after <see cref="DespawnTime"/> seconds.
/// </summary>
public partial class DroppedItem : RigidBody3D
{
    /// <summary>Item type identifier (must exist in <see cref="ItemRegistry"/>).</summary>
    [Export]
    public string ItemId { get; set; } = "";

    /// <summary>Number of items in this stack.</summary>
    [Export]
    public int Amount { get; set; } = 1;

    /// <summary>Distance within which the player picks up this item.</summary>
    [Export]
    public float PickupRadius { get; set; } = 1.5f;

    /// <summary>Seconds before the item becomes pickup-able (gives time to see the bounce).</summary>
    [Export]
    public float PickupDelay { get; set; } = 0.8f;

    /// <summary>Seconds before the item despawns if not picked up.</summary>
    [Export]
    public float DespawnTime { get; set; } = 300f;

    /// <summary>
    /// The player who can pick up this item. Set at spawn time, not via [Export]
    /// because it references a runtime object outside the scene.
    /// </summary>
    public Player? Target { get; set; }

    private float _age;
    private bool _canPickup;

    public override void _Ready()
    {
        // Apply a small random bounce so the item pops out of the dead entity
        var impulse = new Vector3(
            (float)(GD.Randf() - 0.5f) * 2f,
            (float)GD.Randf() * 4f + 2f,
            (float)(GD.Randf() - 0.5f) * 2f
        );
        ApplyCentralImpulse(impulse);

        // Freeze rotation after a moment so the item sits upright on the ground
    }

    public override void _Process(double delta)
    {
        _age += (float)delta;

        if (!_canPickup && _age >= PickupDelay)
            _canPickup = true;

        // Check pickup proximity
        if (_canPickup && Target != null)
        {
            float dist = GlobalPosition.DistanceTo(Target.GlobalPosition);
            if (dist < PickupRadius)
                TryPickup();
        }

        // Auto-despawn
        if (_age >= DespawnTime)
            QueueFree();
    }

    private void TryPickup()
    {
        if (Target == null || string.IsNullOrEmpty(ItemId)) return;

        int leftover = Target.Inventory.AddItem(ItemId, Amount);
        if (leftover <= 0)
        {
            QueueFree();
        }
        else
        {
            Amount = leftover;
            _age = 0f; // reset despawn timer so the player has more time to make space
        }
    }
}
