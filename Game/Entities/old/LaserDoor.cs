using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LaserDoor : Node3D
{
    [Export]
    public AnimatableBody3D DoorBody { get; set; } = null!;

    [Export]
    public CollisionShape3D DoorCollision { get; set; } = null!;

    [Export]
    public Vector3 OpenOffset { get; set; } = new(0, 2.6f, 0);

    [Export]
    public double MoveDuration { get; set; } = 0.35;

    public bool IsOpen { get; private set; }

    Vector3 closedPosition;

    Tween? moveTween;

    public override void _Ready()
    {
        if (IsInstanceValid(DoorBody))
        {
            closedPosition = DoorBody.Position;
        }
    }

    public void SetPowered(bool powered)
    {
        if (IsOpen == powered)
        {
            return;
        }

        IsOpen = powered;
        MoveDoor(powered);
    }

    void MoveDoor(bool open)
    {
        if (!IsInstanceValid(DoorBody))
        {
            return;
        }

        moveTween?.Kill();

        var target = open ? closedPosition + OpenOffset : closedPosition;

        if (IsInstanceValid(DoorCollision))
        {
            DoorCollision.Disabled = open;
        }

        moveTween = CreateTween();
        moveTween.SetEase(Tween.EaseType.Out);
        moveTween.SetTrans(Tween.TransitionType.Cubic);
        moveTween.TweenProperty(DoorBody, "position", target, MoveDuration);
    }
}
