using System;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class Projectile : RigidBody3D
{
    [Export]
    public int Damage { get; set; }
}
