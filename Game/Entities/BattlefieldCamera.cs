using System;
using System.Collections.Generic;
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
    public float PanSpeed { get; set; } = 0.001f;

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
    bool startedDrag = false;
    Vector2 startDragPos;

    List<UnitBase> SelectedUnits { get; set; } = new();

    List<MeshInstance3D> Markers { get; set; } = new();

    [Export]
    PackedScene markerMesh = null!;

    [Export]
    NinePatchRect dragRect = null!;

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

                    case MouseButton.Left:
                        if (!startedDrag)
                            startDragPos = buttonEvent.Position;
                        startedDrag = true;
                        break;
                    case MouseButton.Right:
                        var clickPos = MainCamera.ProjectPosition(
                            buttonEvent.Position,
                            MainCamera.GlobalPosition.Y
                        );
                        int idx = 0;
                        foreach (var unit in SelectedUnits)
                        {
                            unit.SetTargetPos(clickPos with { Y = unit.GlobalPosition.Y });
                            Markers[idx].GlobalPosition = clickPos with
                            {
                                Y = unit.GlobalPosition.Y + 0.02f
                            };
                            idx++;
                        }
                        break;
                }
            else
            {
                switch (buttonEvent.ButtonIndex)
                {
                    case MouseButton.Middle:
                        mouseMoving = false;
                        break;
                    case MouseButton.Left:
                        if (startedDrag)
                        {
                            // GD.Print("just ended drag");
                            // var endPos = buttonEvent.Position;

                            Rect2 visualShape = (new Rect2(startDragPos, Vector2.Zero)).Expand(
                                buttonEvent.Position
                            );

                            // var startWorldPos = MainCamera.ProjectPosition(
                            //     startDragPos,
                            //     MainCamera.GlobalPosition.Y
                            // );
                            // var endWorldPos = MainCamera.ProjectPosition(
                            //     endPos,
                            //     MainCamera.GlobalPosition.Y
                            // );
                            // Rect2 shape = (
                            //     new Rect2(new(startWorldPos.X, startWorldPos.Z), Vector2.Zero)
                            // ).Expand(new(endWorldPos.X, endWorldPos.Z));

                            // GD.Print();
                            SelectedUnits.Clear();
                            foreach (var node in Markers)
                            {
                                node.QueueFree();
                            }
                            Markers.Clear();
                            foreach (var node in GetTree().CurrentScene.GetChildren())
                            {
                                if (
                                    node is UnitBase unit
                                    && visualShape.HasPoint(
                                        MainCamera.UnprojectPosition(unit.GlobalPosition)
                                    )
                                )
                                {
                                    SelectedUnits.Add(unit);
                                    var newMarker = markerMesh.Instantiate<MeshInstance3D>();
                                    GetTree().CurrentScene.AddChild(newMarker);
                                    newMarker.GlobalPosition = unit.GlobalPosition with
                                    {
                                        Y = unit.GlobalPosition.Y + 0.02f
                                    };
                                    Markers.Add(newMarker);
                                }
                            }
                        }
                        startedDrag = false;
                        dragRect.Hide();

                        break;
                }
            }
        }

        if (@event is InputEventMouseMotion motion)
        {
            if (mouseMoving)
            {
                var sensitivity = PanSpeed * targetPos.Y;

                var movement = new Vector3(
                    -motion.Relative.X * sensitivity,
                    0,
                    -motion.Relative.Y * sensitivity
                );

                targetPos += movement;
            }

            if (startedDrag)
            {
                Rect2 visualShape = (new Rect2(startDragPos, Vector2.Zero)).Expand(motion.Position);
                dragRect.Position = visualShape.Position;
                dragRect.Size = visualShape.Size / dragRect.Scale;
                dragRect.Show();
            }
        }
    }
}
