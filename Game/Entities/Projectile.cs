using System;
using Godot;
using GodotTask;

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

    public string SourceFactionID { get; set; } = "";

    [Export]
    public float DespawnTime { get; set; } = 10f;

    public override void _Ready()
    {
        BodyEntered += Detect;
    }

    [Export]
    PackedScene explosion = null!;

    private void Detect(Node body)
    {
        var exp = explosion.Instantiate<Node3D>();
        GetTree().CurrentScene.AddChild(exp);
        exp.GlobalPosition = this.GlobalPosition;

        DespawnSoon(exp).Forget();
    }

    async GDTaskVoid DespawnSoon(Node3D exp)
    {
        await GDTask.Delay(25);
        if (GodotObject.IsInstanceValid(this))
            this.QueueFree();

        await GDTask.Delay(500);
        exp.QueueFree();
    }
}
