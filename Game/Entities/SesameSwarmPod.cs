using System;
using Godot;
using GodotTask;

namespace Game.Entities;

[GlobalClass]
public partial class SesameSwarmPod : UnitBase
{
    [Export]
    public PackedScene SwarmUnitScene { get; set; } = null!;

    [Export]
    public int SpawnCount { get; set; } = 5;

    [Export]
    public float SpawnIntervalSeconds { get; set; } = 0.4f;

    [Export]
    public bool RepeatSpawnSessions { get; set; } = true;

    [Export]
    public float SpawnSessionIntervalSeconds { get; set; } = 6f;

    [Export]
    public float SpawnRadius { get; set; } = 1.8f;

    bool destroyed = false;

    public override void _Ready()
    {
        base._Ready();
        SpawnSwarmLoop().Forget();
    }

    async GDTaskVoid SpawnSwarmLoop()
    {
        var rng = new RandomNumberGenerator();
        while (!destroyed && GodotObject.IsInstanceValid(this))
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                if (destroyed || !GodotObject.IsInstanceValid(this))
                    return;

                if (i > 0)
                    await GDTask.Delay(TimeSpan.FromSeconds(SpawnIntervalSeconds));

                if (destroyed || !GodotObject.IsInstanceValid(this))
                    return;

                var spawned = SwarmUnitScene.Instantiate<SesameSwarmUnit>();
                var spawnOffset = new Vector3(
                    rng.RandfRange(-SpawnRadius, SpawnRadius),
                    0,
                    rng.RandfRange(-SpawnRadius, SpawnRadius)
                );
                GetTree().CurrentScene.AddChild(spawned);
                spawned.GlobalPosition = GlobalPosition + spawnOffset;
                spawned.FactionID = FactionID;
            }

            if (!RepeatSpawnSessions)
                return;

            await GDTask.Delay(TimeSpan.FromSeconds(Math.Max(0f, SpawnSessionIntervalSeconds)));
        }
    }

    public override void _ExitTree()
    {
        destroyed = true;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        state.LinearVelocity = state.LinearVelocity with { X = 0, Z = 0 };
        state.AngularVelocity = Vector3.Zero;
    }
}
