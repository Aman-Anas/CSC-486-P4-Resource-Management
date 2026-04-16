using System;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class UnitBehavior : Resource
{
    [Export]
    public int InitialHealth { get; set; } = 100;

    /// <summary>
    /// Scene used for instancing projectile
    /// </summary>
    [Export]
    public PackedScene Projectile { get; set; } = null!;

    /// <summary>
    /// The range within which units respond to enemies
    /// </summary>
    [Export]
    public float ResponseRadius { get; set; } = 10f;

    /// <summary>
    /// The time it takes for a unit to reload
    /// </summary>
    [Export]
    public float ReloadTime { get; set; } = 0.5f;

    /// <summary>
    /// Speed at which a unit moves
    /// </summary>
    [Export]
    public float MoveSpeed { get; set; } = 1.0f;

    [Export]
    public float MoveForce { get; set; } = 15f;

    [Export]
    public float StopDistance { get; set; } = 3f;
}
