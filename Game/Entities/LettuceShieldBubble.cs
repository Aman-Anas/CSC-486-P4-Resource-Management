using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LettuceShieldBubble : Area3D
{
    [Export]
    public CollisionShape3D CollisionShape { get; set; } = null!;

    [Export]
    public float Radius { get; set; } = 6f;

    [Export]
    public LettuceGenerator Generator { get; set; } = null!;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        ApplyRadius();
        BodyEntered += HandleBodyEntered;
    }

    public void ApplyRadius()
    {
        if (CollisionShape == null)
            return;

        CollisionShape.Shape = new SphereShape3D() { Radius = Radius };
    }

    void HandleBodyEntered(Node3D body)
    {
        TryConsumeDamageSource(body);
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Node3D body3D)
                TryConsumeDamageSource(body3D);
        }
    }

    void TryConsumeDamageSource(Node3D body)
    {
        if (Generator == null || !GodotObject.IsInstanceValid(Generator))
            return;

        if (body is Projectile projectile)
        {
            if (projectile.SourceFactionID == Generator.FactionID)
                return;

            Generator.ApplyShieldDamage(projectile.Damage);
            projectile.QueueFree();
            return;
        }

        if (body is ICauseDamage source)
        {
            if (body is UnitBase unit && unit.FactionID == Generator.FactionID)
                return;

            Generator.ApplyShieldDamage(source.Damage);
            body.QueueFree();
        }
    }
}
