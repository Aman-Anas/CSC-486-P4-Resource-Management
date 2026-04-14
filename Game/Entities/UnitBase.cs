using System;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class UnitBase : RigidBody3D
{
    [Export]
    Area3D DetectionArea { get; set; } = null!;

    [Export]
    public UnitBehavior Behavior { get; set; } = null!;

    UnitBase? followingEnemy = null;

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        // If we've been following an enemy, check if it's still within sight range
        if (
            followingEnemy != null
            && GlobalPosition.DistanceTo(followingEnemy.GlobalPosition) > (Behavior.ResponseRadius)
        )
        {
            followingEnemy = null;
        }

        if (followingEnemy == null)
        {
            // Scan the surroundings for enemies if we don't have a target yet
            foreach (var obj in DetectionArea.GetOverlappingBodies())
            {
                if (obj is UnitBase unit)
                {
                    followingEnemy = unit;
                    break;
                }
            }
        }

        Vector3 velo;
        // At this point we should have an enemy target if there's one in range.
        if (followingEnemy != null)
        {
            this.LookAt(followingEnemy.GlobalPosition);
            velo = new(0, 0, -Behavior.MoveSpeed);
        }
        else
        {
            velo = Vector3.Zero;
        }

        velo += state.TotalGravity;

        state.LinearVelocity = velo;
    }
}
