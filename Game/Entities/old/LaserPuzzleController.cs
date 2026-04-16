using System.Collections.Generic;
using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class LaserPuzzleController : Node3D
{
    [Export]
    public LaserEmitter Emitter { get; set; } = null!;

    [Export]
    public LaserReceiver Receiver { get; set; } = null!;

    [Export]
    public LaserDoor Door { get; set; } = null!;

    [Export]
    public Node3D BeamRoot { get; set; } = null!;

    [Export]
    public int MaxBounces { get; set; } = 800000;

    [Export]
    public float MaxBeamDistance { get; set; } = 400000f;

    [Export]
    public float BeamRadius { get; set; } = 0.03f;

    [Export]
    public Color BeamColor { get; set; } = new(1f, 0.25f, 0.2f, 1f);

    readonly Dictionary<Farmer, LaserMirror> carriedMirrorByFarmer = new();
    readonly List<MeshInstance3D> beamSegments = new();

    StandardMaterial3D beamMaterial = null!;

    int usedSegmentCount;

    public override void _Ready()
    {
        if (!IsInstanceValid(BeamRoot))
        {
            BeamRoot = new Node3D { Name = "BeamSegments", };
            AddChild(BeamRoot);
        }

        beamMaterial = new StandardMaterial3D
        {
            ResourceLocalToScene = true,
            AlbedoColor = BeamColor,
            EmissionEnabled = true,
            Emission = BeamColor,
            EmissionEnergyMultiplier = 3.0f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateBeamAndPuzzleState();
    }

    public bool IsCarryingMirror(Farmer farmer)
    {
        return carriedMirrorByFarmer.ContainsKey(farmer);
    }

    public LaserMirror? GetCarriedMirror(Farmer farmer)
    {
        if (carriedMirrorByFarmer.TryGetValue(farmer, out var mirror))
        {
            return mirror;
        }

        return null;
    }

    public bool TryPickupMirror(Farmer farmer, LaserMirror mirror)
    {
        if (carriedMirrorByFarmer.ContainsKey(farmer) || mirror.IsCarried)
        {
            return false;
        }

        mirror.CurrentSocket?.SetOccupyingMirror(null);
        mirror.ClearSocketReference();

        mirror.SetCarriedState(true);
        carriedMirrorByFarmer[farmer] = mirror;

        return true;
    }

    public bool TryPlaceCarriedMirror(Farmer farmer, LaserMirrorSocket socket)
    {
        if (socket.OccupyingMirror != null)
        {
            return false;
        }

        if (!carriedMirrorByFarmer.TryGetValue(farmer, out var mirror))
        {
            return false;
        }

        carriedMirrorByFarmer.Remove(farmer);

        socket.SetOccupyingMirror(mirror);
        mirror.PlaceOnSocket(socket);

        return true;
    }

    void UpdateBeamAndPuzzleState()
    {
        if (!IsInstanceValid(Emitter) || !IsInstanceValid(Receiver) || !IsInstanceValid(Door))
        {
            return;
        }

        usedSegmentCount = 0;

        var emitterOrigin = Emitter.GetBeamOrigin();
        var emitterDirection = Emitter.GetBeamDirection().Normalized();

        var hitReceiver = TraceBeam(emitterOrigin, emitterDirection, MaxBeamDistance, 0);

        for (var i = usedSegmentCount; i < beamSegments.Count; i++)
        {
            beamSegments[i].Visible = false;
        }

        Receiver.SetPowered(hitReceiver);
        Door.SetPowered(hitReceiver);
    }

    bool TraceBeam(Vector3 origin, Vector3 direction, float remainingDistance, int bounceCount)
    {
        if (remainingDistance <= 0.05f)
        {
            return false;
        }

        var start = origin + direction * 0.02f;
        var end = start + direction * remainingDistance;

        var query = PhysicsRayQueryParameters3D.Create(start, end);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        if (IsInstanceValid(Emitter.CollisionBody))
        {
            var exclude = new Godot.Collections.Array<Rid> { Emitter.CollisionBody.GetRid(), };
            query.Exclude = exclude;
        }

        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);

        if (hit.Count == 0)
        {
            AddBeamSegment(start, end);
            return false;
        }

        var hitPosition = (Vector3)hit["position"];
        AddBeamSegment(start, hitPosition);

        if (!hit.ContainsKey("collider"))
        {
            return false;
        }

        var colliderObject = hit["collider"].AsGodotObject();
        if (colliderObject is not Node colliderNode)
        {
            return false;
        }

        var receiver = FindAncestorOfType<LaserReceiver>(colliderNode);
        if (receiver != null)
        {
            return true;
        }

        var mirror = FindAncestorOfType<LaserMirror>(colliderNode);
        if (mirror != null && bounceCount < MaxBounces)
        {
            var normal = (Vector3)hit["normal"];
            var reflected = direction.Bounce(normal).Normalized();

            if (reflected.LengthSquared() > 0.001f)
            {
                var traveled = start.DistanceTo(hitPosition);
                var remaining = remainingDistance - traveled;

                return TraceBeam(hitPosition, reflected, remaining, bounceCount + 1);
            }
        }

        return false;
    }

    static T? FindAncestorOfType<T>(Node? node)
        where T : class
    {
        var current = node;

        while (current != null)
        {
            if (current is T found)
            {
                return found;
            }

            current = current.GetParent();
        }

        return null;
    }

    void AddBeamSegment(Vector3 start, Vector3 end)
    {
        var length = start.DistanceTo(end);

        if (length <= 0.02f)
        {
            return;
        }

        MeshInstance3D segment;
        if (usedSegmentCount >= beamSegments.Count)
        {
            segment = CreateSegment();
            beamSegments.Add(segment);
        }
        else
        {
            segment = beamSegments[usedSegmentCount];
        }

        usedSegmentCount++;

        segment.Visible = true;

        var direction = (end - start).Normalized();
        var midpoint = (start + end) * 0.5f;

        segment.GlobalTransform = new Transform3D(
            new Basis(new Quaternion(Vector3.Up, direction)),
            midpoint
        );
        segment.Scale = new Vector3(BeamRadius * 2f, length, BeamRadius * 2f);
    }

    MeshInstance3D CreateSegment()
    {
        var segment = new MeshInstance3D
        {
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Mesh = new CylinderMesh
            {
                Height = 1f,
                TopRadius = 0.5f,
                BottomRadius = 0.5f,
                RadialSegments = 10,
            },
        };

        segment.SetSurfaceOverrideMaterial(0, beamMaterial);
        BeamRoot.AddChild(segment);

        return segment;
    }
}
