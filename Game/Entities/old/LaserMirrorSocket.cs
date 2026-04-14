using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LaserMirrorSocket : Node3D, IPlayerInteractable
{
    [Export]
    public LaserPuzzleController Controller { get; set; } = null!;

    [Export]
    public Node3D SnapPoint { get; set; } = null!;

    [Export]
    public Color InteractionColor { get; set; } = new(0.85f, 1f, 0.75f, 1f);

    public LaserMirror? OccupyingMirror { get; private set; }

    public string GetInteractionText(Farmer farmer)
    {
        if (OccupyingMirror != null)
        {
            return string.Empty;
        }

        if (!Controller.IsCarryingMirror(farmer))
        {
            return string.Empty;
        }

        return "[e] place mirror block";
    }

    public Color GetInteractionColor()
    {
        return InteractionColor;
    }

    public void Interact(Farmer farmer)
    {
        if (OccupyingMirror != null)
        {
            return;
        }

        Controller.TryPlaceCarriedMirror(farmer, this);
    }

    public Transform3D GetSnapTransform()
    {
        if (IsInstanceValid(SnapPoint))
        {
            return SnapPoint.GlobalTransform;
        }

        return GlobalTransform;
    }

    public void SetOccupyingMirror(LaserMirror? mirror)
    {
        OccupyingMirror = mirror;
    }
}
