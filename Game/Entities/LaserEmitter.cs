using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LaserEmitter : Node3D, IPlayerInteractable
{
    [Export]
    public Node3D BeamOrigin { get; set; } = null!;

    [Export]
    public StaticBody3D CollisionBody { get; set; } = null!;

    [Export]
    public float RotationStepDegrees { get; set; } = 2f;

    [Export]
    public Color InteractionColor { get; set; } = new(1f, 0.45f, 0.2f, 1f);

    [Export]
    public bool Enabled { get; set; } = true;

    public Vector3 GetBeamOrigin()
    {
        return IsInstanceValid(BeamOrigin) ? BeamOrigin.GlobalPosition : GlobalPosition;
    }

    public Vector3 GetBeamDirection()
    {
        // In Godot, forward is -Z.
        return -GlobalBasis.Z;
    }

    public string GetInteractionText(Farmer farmer)
    {
        if (!Enabled)
        {
            return "Laser emitter is disabled.";
        }

        return "[e] rotate laser emitter";
    }

    public Color GetInteractionColor()
    {
        return InteractionColor;
    }

    public void Interact(Farmer farmer)
    {
        if (!Enabled)
        {
            return;
        }

        RotateY(Mathf.DegToRad(RotationStepDegrees));
    }
}
