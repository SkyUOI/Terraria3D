using Godot;

namespace Terraria3D.entities.components;

/// <summary>States every AI entity can be in.</summary>
public enum AiState
{
    Idle,
    Wander,
    Chase,
    Attack,
    Flee,
    Die,
}

/// <summary>
/// Finite-state-machine driver for entity AI. Attached as a child Node of
/// any <see cref="Entity"/>. Evaluates transitions each physics frame and
/// delegates movement/attack calls to the owning entity.
/// </summary>
public partial class AiController : Node
{
    /// <summary>The entity this AI controls. Set via editor or code.</summary>
    [Export]
    public Entity Entity { get; set; }

    /// <summary>Distance at which the entity notices the player.</summary>
    [Export]
    public float AggroRange { get; set; } = 12.0f;

    /// <summary>Distance at which the entity can attack.</summary>
    [Export]
    public float AttackRange { get; set; } = 2.0f;

    /// <summary>Hysteresis multiplier — lose interest when target is this far beyond aggro range.</summary>
    [Export]
    public float LoseInterestMultiplier { get; set; } = 1.5f;

    /// <summary>Minimum seconds spent in Idle before transitioning.</summary>
    [Export]
    public float IdleTimeMin { get; set; } = 1.0f;

    /// <summary>Maximum seconds spent in Idle before transitioning.</summary>
    [Export]
    public float IdleTimeMax { get; set; } = 3.0f;

    /// <summary>Minimum seconds spent in Wander before transitioning.</summary>
    [Export]
    public float WanderTimeMin { get; set; } = 2.0f;

    /// <summary>Maximum seconds spent in Wander before transitioning.</summary>
    [Export]
    public float WanderTimeMax { get; set; } = 5.0f;

    public AiState CurrentState { get; private set; } = AiState.Idle;

    private float _stateTimer;
    private float _currentStateDuration;
    private Vector3 _wanderDirection;
    private IDamageable _target;
    private Node3D _targetNode;

    public override void _Ready()
    {
        base._Ready();
        _currentStateDuration = RandomDuration(IdleTimeMin, IdleTimeMax);

        // Find the player (assumed to be in group "player")
        var players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0 && players[0] is IDamageable damageable)
        {
            _target = damageable;
            _targetNode = players[0] as Node3D;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Entity == null || Entity.IsDead)
        {
            if (CurrentState != AiState.Die)
                TransitionTo(AiState.Die);
            return;
        }

        EvaluateTransitions((float)delta);
        ExecuteState((float)delta);
    }

    private void EvaluateTransitions(float delta)
    {
        _stateTimer += delta;
        float distToTarget = DistanceToTarget();

        switch (CurrentState)
        {
            case AiState.Idle:
                if (_stateTimer >= _currentStateDuration)
                    TransitionTo(AiState.Wander);
                else if (distToTarget < AggroRange)
                    TransitionTo(AiState.Chase);
                break;

            case AiState.Wander:
                if (_stateTimer >= _currentStateDuration)
                    TransitionTo(AiState.Idle);
                else if (distToTarget < AggroRange)
                    TransitionTo(AiState.Chase);
                break;

            case AiState.Chase:
                if (distToTarget > AggroRange * LoseInterestMultiplier)
                    TransitionTo(AiState.Wander);
                else if (distToTarget < AttackRange)
                    TransitionTo(AiState.Attack);
                break;

            case AiState.Attack:
                if (distToTarget > AttackRange * 1.2f)
                    TransitionTo(AiState.Chase);
                break;

            case AiState.Flee:
                if (distToTarget > AggroRange * 2f)
                    TransitionTo(AiState.Wander);
                break;

            case AiState.Die:
                break;
        }
    }

    private void ExecuteState(float delta)
    {
        switch (CurrentState)
        {
            case AiState.Idle:
                Entity.Velocity = Vector3.Zero;
                break;

            case AiState.Wander:
                Entity.MoveInDirection(_wanderDirection, delta);
                if (_stateTimer < 0.5f && Entity.Velocity.Length() < 0.1f)
                    PickNewWanderDirection();
                break;

            case AiState.Chase:
                if (_targetNode != null)
                    Entity.MoveToward(_targetNode.GlobalPosition, delta);
                break;

            case AiState.Attack:
                Entity.PerformAttack(_target, delta);
                break;

            case AiState.Flee:
                if (_targetNode != null)
                    Entity.MoveToward(Entity.GlobalPosition * 2 - _targetNode.GlobalPosition, delta);
                break;

            case AiState.Die:
                Entity.Velocity = Vector3.Zero;
                break;
        }
    }

    private void TransitionTo(AiState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        _stateTimer = 0f;

        _currentStateDuration = newState switch
        {
            AiState.Idle => RandomDuration(IdleTimeMin, IdleTimeMax),
            AiState.Wander => RandomDuration(WanderTimeMin, WanderTimeMax),
            _ => float.MaxValue,
        };

        if (newState == AiState.Wander)
            PickNewWanderDirection();

        GD.Print($"[AI] {Entity?.GetType().Name} → {newState}");
    }

    private void PickNewWanderDirection()
    {
        float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
        _wanderDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).Normalized();
    }

    private float DistanceToTarget()
    {
        if (_targetNode == null || (_target != null && _target.IsDead))
            return float.MaxValue;
        return Entity.GlobalPosition.DistanceTo(_targetNode.GlobalPosition);
    }

    private static float RandomDuration(float min, float max)
    {
        return (float)GD.RandRange(min, max);
    }
}
