using System;
using Godot;

[Tool]
public partial class PodGenerator : Node3D
{
    [Export] public PackedScene SeatPodScene { get; set; } = null!;
    [Export] public Node3D SeatContainer { get; set; } = null!;
    [Export] public int SeatsInRow { get; set; } = 24;
    [Export] public float FirstRowRadius { get; set; } = 20.0f;
    [Export] public float RowVerticalSpacing { get; set; } = 2.0f;
    [Export] public float RowHorizontalSpacing { get; set; } = 1.5f;
    [Export] public int NumRows { get; set; } = 10;
    [Export] public bool RebuildNow { get; set; } = false;

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

        for (int row = 0; row < NumRows; row++)
        {
            float rowY = row * RowVerticalSpacing;
            float radius = FirstRowRadius + row * RowHorizontalSpacing;

            float angleOffset = row % 2 == 0 ? 0 : 0.5f;

            for (int i = 0; i < SeatsInRow; i++)
            {
                float angle = 2 * Mathf.Pi * (i + angleOffset) / SeatsInRow;
                var seat = SeatPodScene.Instantiate<Node3D>();
                seat.Position = new Vector3(radius * Mathf.Cos(angle), rowY, radius * Mathf.Sin(angle));
                seat.Rotation = new Vector3(0, -angle - Mathf.Pi / 2, 0);
                seat.Name = "Seat" + ++numSeats;
                SeatContainer.AddChild(seat);
                seat.Owner = editedRoot;
            }

        }

        GD.Print("[PodGenerator] Rebuilt " + numSeats.ToString() + " seats.");
    }
}
