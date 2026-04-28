using System;
using Godot;

[Tool]
public partial class PodGenerator : Node3D
{
    [Export] public PackedScene SeatPodScene { get; set; } = null!;
    [Export] public bool RebuildNow { get; set; } = false;
    [Export] public Node3D SeatContainer { get; set; } = null!;
    [Export] public int SeatsInRow { get; set; } = 10;
    [Export] public float FirstRowRadius { get; set; } = 20.0f;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint())
            return;

        if (RebuildNow)
        {
            RebuildNow = false;
            GenerateSeats();
        }
    }

    void GenerateSeats()
    {
        if (SeatPodScene == null)
        {
            GD.PushWarning("[PodGenerator] SeatPodScene is not assigned.");
            return;
        }

        if (SeatContainer == null)
        {
            GD.PushWarning("[PodGenerator] SeatContainer is not assigned.");
            return;
        }

        foreach (Node child in SeatContainer.GetChildren())
            child.QueueFree();

        var editedRoot = GetTree().EditedSceneRoot;
        int numSeats = 0;

        for (int i = 0; i < SeatsInRow; i++)
        {
            float angle = 2 * Mathf.Pi * i / SeatsInRow;
            var seat = SeatPodScene.Instantiate<Node3D>();
            seat.Position = new Vector3(FirstRowRadius * Mathf.Cos(angle), 0, FirstRowRadius * Mathf.Sin(angle));
            seat.Rotation = new Vector3(0, -angle - Mathf.Pi / 2, 0);
            seat.Name = "Seat" + ++numSeats;
            SeatContainer.AddChild(seat);
            seat.Owner = editedRoot;
        }

        GD.Print("[PodGenerator] Rebuilt {0} seats.", numSeats);
    }
}
