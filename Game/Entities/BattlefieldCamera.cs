using System;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class BattlefieldCamera : Node3D
{
    [Export]
    public Camera3D MainCamera { get; set; } = null!;

    [Export]
    public float ZoomSpeed { get; set; } = 5f;

    Vector3 targetPos;

    public override void _Ready()
    {
        targetPos = MainCamera.GlobalPosition;
    }

    public override void _Process(double delta)
    {
        var movementVec = Input.GetVector(
            GameActions.PlayerStrafeLeft,
            GameActions.PlayerStrafeRight,
            GameActions.PlayerBackward,
            GameActions.PlayerForward
        );

        targetPos.X += (float)(movementVec.X * delta);
        targetPos.Z += (float)(movementVec.Y * delta);

        MainCamera.GlobalPosition = MainCamera.GlobalPosition.Lerp(
            targetPos,
            (float)((0.1) * delta)
        );
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton buttonEvent)
        {
            switch (buttonEvent.ButtonIndex)
            {
                case MouseButton.WheelUp:
                    targetPos.Y -= ZoomSpeed;
                    break;
                // make it go down
                case MouseButton.WheelDown:
                    // go up

                    targetPos.Y += ZoomSpeed;
                    break;
            }
        }
    }
}
