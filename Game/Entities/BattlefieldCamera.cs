using System;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class BattlefieldCamera : Node3D
{
    [Export]
    public Camera3D MainCamera { get; set; } = null!;

    [Export]
    public float MoveSpeed { get; set; } = 20f;

    [Export]
    public float ZoomSpeed { get; set; } = 2f;

    Vector3 targetPos;

    public override void _Ready()
    {
        targetPos = MainCamera.GlobalPosition;
    }

    public override void _Process(double delta)
    {
        var movementVec =
            Input.GetVector(
                GameActions.PlayerStrafeLeft,
                GameActions.PlayerStrafeRight,
                GameActions.PlayerBackward,
                GameActions.PlayerForward
            ) * MoveSpeed;

        targetPos.X += (float)(movementVec.X * delta);
        targetPos.Z += -(float)(movementVec.Y * delta);

        MainCamera.GlobalPosition = targetPos; //Damp(MainCamera.GlobalPosition, targetPos, 0.9f, (float)delta);
    }

    public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt)
    {
        return a.Lerp(b, 1 - Mathf.Exp(-lambda * dt));
    }

    bool mouseMoving = false;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton buttonEvent)
        {
            if (buttonEvent.Pressed)
                switch (buttonEvent.ButtonIndex)
                {
                    case MouseButton.WheelUp:
                        if (targetPos.Y > ZoomSpeed)
                            // make it go down
                            targetPos.Y -= ZoomSpeed;
                        break;

                    case MouseButton.WheelDown:
                        // go up
                        targetPos.Y += ZoomSpeed;
                        break;
                    case MouseButton.Middle:
                        mouseMoving = true;
                        break;
                }
            else if (buttonEvent.ButtonIndex == MouseButton.Middle)
            {
                mouseMoving = false;
            }
        }

        if (mouseMoving && @event is InputEventMouseMotion motion)
        {
            var sensitivity = Manager.Instance.Config.MouseSensitivity * targetPos.Y;

            var movement = new Vector3(
                -motion.Relative.X * sensitivity,
                0,
                -motion.Relative.Y * sensitivity
            );

            targetPos += movement;
        }
    }
}
