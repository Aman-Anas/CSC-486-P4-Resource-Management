using System;
using Game.UI;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class UnitBase : RigidBody3D
{
    [Export]
    FancyProgressBar healthBar = null!;

    [Export]
    Area3D DetectionArea { get; set; } = null!;

    [Export]
    CollisionShape3D DetectionShape { get; set; } = null!;

    [Export]
    public UnitBehavior Behavior { get; set; } = null!;

    [Export]
    public string FactionID { get; set; } = "";

    [Export]
    public int Health { get; set; }

    UnitBase? followingEnemy = null;

    public override void _Ready()
    {
        this.DetectionShape.Shape = new SphereShape3D() { Radius = Behavior.ResponseRadius };

        // Setup health
        Health = Behavior.InitialHealth;
        healthBar.MaxValue = Behavior.InitialHealth;

        this.BodyEntered += DetectedCollision;
    }

    private void DetectedCollision(Node body)
    {
        if (body is Projectile proj)
        {
            Health -= proj.Damage;
            healthBar.SetCoolValue(Health);
            if (Health <= 0)
            {
                this.QueueFree();
            }
        }
    }

    enum State
    {
        Engaged,
        Idle,
        MovingToPos
    }

    State currentState = State.Idle;

    Vector3 targetPosition;

    public void SetTargetPos(in Vector3 pos)
    {
        targetPosition = pos;
        currentState = State.MovingToPos;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        Vector3 intendedVelocity = Vector3.Zero;

        switch (currentState)
        {
            case State.Idle:
                // Scan the surroundings for enemies if we don't have a target yet
                foreach (var obj in DetectionArea.GetOverlappingBodies())
                {
                    if (obj is UnitBase unit && unit.FactionID != this.FactionID)
                    {
                        followingEnemy = unit;
                        currentState = State.Engaged;
                        break;
                    }
                }
                intendedVelocity = Vector3.Zero;
                break;

            case State.Engaged:
                // Make sure the follow target is valid. If the enemy died etc.
                // then we should go back to idle.
                if (
                    followingEnemy == null
                    || !GodotObject.IsInstanceValid(followingEnemy)
                    // Also check if it's still within sight range
                    || GlobalPosition.DistanceTo(followingEnemy.GlobalPosition)
                        > (Behavior.ResponseRadius)
                )
                {
                    currentState = State.Idle;
                    followingEnemy = null;
                    break;
                }

                this.LookAt(followingEnemy.GlobalPosition);
                intendedVelocity = new(0, 0, -Behavior.MoveSpeed);

                break;

            case State.MovingToPos:
                if (this.GlobalPosition.DistanceTo(targetPosition) < 2)
                {
                    currentState = State.Idle;
                    break;
                }

                this.LookAt(targetPosition);
                intendedVelocity = new(0, 0, -Behavior.MoveSpeed);
                break;
        }

        Vector3 currentVelo = state.LinearVelocity with { Y = 0 };
        var diff = (GlobalBasis * intendedVelocity) - currentVelo;

        var forceVec = diff.LimitLength(Behavior.MoveSpeed);
        state.ApplyCentralForce(forceVec);
    }
}
