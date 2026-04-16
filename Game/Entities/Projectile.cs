using System;
using Godot;

namespace Game.Entities;

public interface ICauseDamage
{
    public int Damage { get; set; }
}

[GlobalClass]
public partial class Projectile : RigidBody3D, ICauseDamage
{
    [Export]
    public int Damage { get; set; }

    [Export]
    public float DespawnTime { get; set; } = 10f;
}
