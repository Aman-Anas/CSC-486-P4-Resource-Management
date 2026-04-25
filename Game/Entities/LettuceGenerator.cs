using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LettuceGenerator : Node3D
{
    [Export]
    public LettuceShieldBubble ShieldBubble { get; set; } = null!;

    [Export]
    public int MaxShieldHealth { get; set; } = 100;

    [Export]
    public float BubbleRadius { get; set; } = 6f;

    [Export]
    public string FactionID { get; set; } = "";

    public int CurrentShieldHealth { get; private set; }

    bool destroyed = false;

    public override void _Ready()
    {
        CurrentShieldHealth = MaxShieldHealth;

        if (ShieldBubble != null)
        {
            ShieldBubble.Generator = this;
            ShieldBubble.Radius = BubbleRadius;
            ShieldBubble.ApplyRadius();
        }
    }

    public void ApplyShieldDamage(int damage)
    {
        if (destroyed || damage <= 0)
            return;

        CurrentShieldHealth -= damage;
        if (CurrentShieldHealth <= 0)
            DestroyGenerator();
    }

    void DestroyGenerator()
    {
        if (destroyed)
            return;

        destroyed = true;
        if (GodotObject.IsInstanceValid(ShieldBubble))
            ShieldBubble.QueueFree();
        QueueFree();
    }
}
