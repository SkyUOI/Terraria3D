namespace Terraria3D.entities.enemies;

/// <summary>
/// Concrete enemy class. Individual enemy types (Slime, Zombie, etc.) are
/// defined in <see cref="EntityRegistry"/> and share this single class.
/// The <c>TypeId</c> export determines which registry entry to use.
/// Different visuals come from different .tscn files, not different C# classes.
/// </summary>
public partial class Enemy : Entity
{
    public override void _Ready()
    {
        base._Ready();

        // Add to "enemy" group for easy scene-wide queries
        AddToGroup("enemy");
    }
}
