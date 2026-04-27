using System;
using Godot;
using GodotTask;

namespace Game.Entities;

[GlobalClass]
public partial class SesameSwarmUnit : UnitBase, ICauseDamage
{
    [Export]
    public int Damage { get; set; } = 2;

    [Export]
    public float HomingForce { get; set; } = 30f;

    [Export]
    public float MoveSpeed { get; set; } = 6f;

    [Export]
    public float LifetimeSeconds { get; set; } = 10f;

    UnitBase? currentTarget = null;

    public override void _Ready()
    {
        base._Ready();
        DespawnAfterLifetime().Forget();
    }

    async GDTaskVoid DespawnAfterLifetime()
    {
        await GDTask.Delay(TimeSpan.FromSeconds(LifetimeSeconds));
        if (GodotObject.IsInstanceValid(this))
            QueueFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var body in GetCollidingBodies())
        {
            if (body is UnitBase unit && unit.FactionID != FactionID)
            {
                QueueFree();
                return;
            }

            if (body is Projectile projectile && projectile.SourceFactionID != FactionID)
            {
                QueueFree();
                return;
            }
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (
            currentTarget == null
            || !GodotObject.IsInstanceValid(currentTarget)
            || currentTarget.FactionID == FactionID
        )
        {
            currentTarget = FindNearestEnemy();
        }

        if (currentTarget != null && GodotObject.IsInstanceValid(currentTarget))
        {
            var targetPos = currentTarget.GlobalPosition with { Y = GlobalPosition.Y };
            LookAt(targetPos);
            var desiredVelocity = -GlobalBasis.Z * MoveSpeed;
            var currentVelocity = state.LinearVelocity with { Y = 0 };
            var velocityDiff = desiredVelocity - currentVelocity;
            state.ApplyCentralForce(velocityDiff * HomingForce);
        }

        state.AngularVelocity = Vector3.Zero;
    }

    UnitBase? FindNearestEnemy()
    {
        UnitBase? best = null;
        var bestDistance = float.MaxValue;

        foreach (var node in GetTree().CurrentScene.GetChildren())
        {
            if (node is not UnitBase unit || unit == this || unit.FactionID == FactionID)
                continue;

            var dist = GlobalPosition.DistanceTo(unit.GlobalPosition);
            if (dist < bestDistance)
            {
                best = unit;
                bestDistance = dist;
            }
        }

        return best;
    }
}
