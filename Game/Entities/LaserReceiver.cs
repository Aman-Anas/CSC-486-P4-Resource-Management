using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LaserReceiver : Node3D
{
    [Export]
    public MeshInstance3D IndicatorMesh { get; set; } = null!;

    [Export]
    public Color OffColor { get; set; } = new(0.2f, 0.2f, 0.2f, 1f);

    [Export]
    public Color OnColor { get; set; } = new(0.9f, 0.25f, 0.2f, 1f);

    public bool IsPowered { get; private set; }

    StandardMaterial3D indicatorMaterial = null!;

    public override void _Ready()
    {
        if (IsInstanceValid(IndicatorMesh))
        {
            indicatorMaterial = new StandardMaterial3D
            {
                ResourceLocalToScene = true,
                EmissionEnabled = true,
            };

            IndicatorMesh.SetSurfaceOverrideMaterial(0, indicatorMaterial);
        }

        UpdateVisuals();
    }

    public void SetPowered(bool powered)
    {
        if (IsPowered == powered)
        {
            return;
        }

        IsPowered = powered;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (!IsInstanceValid(IndicatorMesh) || indicatorMaterial == null)
        {
            return;
        }

        var color = IsPowered ? OnColor : OffColor;

        indicatorMaterial.AlbedoColor = color;
        indicatorMaterial.Emission = color;
        indicatorMaterial.EmissionEnergyMultiplier = IsPowered ? 2.5f : 0.2f;
    }
}
