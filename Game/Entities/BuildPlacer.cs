using System;
using Game;
using Godot;

namespace Game.Entities;

/// <summary>
/// Hotkey build (1-5) + <see cref="PlaceConfirmKey"/> or <see cref="PlaceButton"/> to confirm at cursor.
/// Uses <see cref="Node._Input"/> and a <see cref="Node._Process"/> fallback so input is not lost to other nodes.
/// </summary>
[GlobalClass]
public partial class BuildPlacer : Node
{
    const uint AllPhysicsLayers = 0b1111_1111_1111_1111_1111_1111_1111_1111u;

    static readonly Key[] s_digitKeys =
    {
        Key.Key1,
        Key.Key2,
        Key.Key3,
        Key.Key4,
        Key.Key5,
        Key.Kp1,
        Key.Kp2,
        Key.Kp3,
        Key.Kp4,
        Key.Kp5
    };

    static readonly (string Path, int Cost)[] s_buildOptions =
    {
        ("res://Scenes/Battlefield/StandardTurret.tscn", 80),
        ("res://Scenes/Battlefield/ShadowBurgerTurret.tscn", 110),
        ("res://Scenes/Battlefield/SesameSwarmPod.tscn", 60),
        ("res://Scenes/Battlefield/LightLettuceGenerator.tscn", 90),
        ("res://Scenes/Battlefield/HeavyLettuceGenerator.tscn", 150)
    };

    /// <summary> HUD / tools: label for slot <c>i</c> (0–4). </summary>
    public static readonly string[] BuildOptionNames =
    {
        "Standard Turret",
        "Shadowburger Turret",
        "Sesame Pod",
        "Light Lettuce Gen",
        "Heavy Lettuce Gen"
    };

    /// <summary> HUD / tools: display name for slot <paramref name="index"/> (0–4). </summary>
    public static string GetBuildOptionName(int index) => BuildOptionNames[index];

    /// <summary> Cost for <paramref name="index"/> (0–4). </summary>
    public static int GetBuildOptionCost(int index) => s_buildOptions[index].Cost;

    [Export]
    public Camera3D MainCamera { get; set; } = null!;

    [Export]
    public string PlayerFactionId { get; set; } = "";

    [Export]
    public int RaycastLength { get; set; } = 20_000;

    /// <summary> If the physics ray hits nothing, intersection with this world Y plane (ground). </summary>
    [Export]
    public float GroundPlaneY { get; set; } = 0f;

    [Export]
    public MouseButton PlaceButton { get; set; } = MouseButton.Xbutton1;

    [Export]
    public Key PlaceConfirmKey { get; set; } = Key.B;

    /// <summary> 0–4 when a build is selected; <see langword="null"/> if none. </summary>
    public int? SelectedBuildIndex => _selectedIndex;

    int? _selectedIndex;
    PackedScene?[]? _scenes;
    bool _placeKeyDown;

    public override void _Ready()
    {
        _scenes = new PackedScene?[s_buildOptions.Length];
        for (var i = 0; i < s_buildOptions.Length; i++)
        {
            var s = ResourceLoader.Load<PackedScene>(s_buildOptions[i].Path);
            _scenes[i] = s;
            if (s == null)
                GD.PrintErr($"[BuildPlacer] Missing scene: {s_buildOptions[i].Path}");
        }

        SetProcessInput(true);
        SetProcess(true);
        ResolveMainCamera();
        // if (MainCamera == null)
        //     GD.PrintErr(
        //         "[BuildPlacer] MainCamera is not set; link Camera3D in the inspector on BuildPlacer."
        //     );
    }

    public override void _EnterTree()
    {
        ResolveMainCamera();
    }

    void ResolveMainCamera()
    {
        MainCamera = GetTree().Root.GetCamera3D();
        // if (MainCamera != null && GodotObject.IsInstanceValid(MainCamera))
        // return;
        // var c = GetNodeOrNull<Camera3D>(new NodePath("../BattlefieldCamera/Camera3D"));
        // if (c != null)
        // MainCamera = c;
    }

    public override void _Process(double delta)
    {
        if (Manager.Instance == null)
        {
            _placeKeyDown = false;
            return;
        }
        if (_selectedIndex == null)
        {
            _placeKeyDown = false;
            return;
        }

        var down = Input.IsPhysicalKeyPressed(PlaceConfirmKey);
        if (down && !_placeKeyDown)
        {
            var v = GetViewport();
            if (v != null && TryPlaceAtScreen(v.GetMousePosition(), out var consume) && consume)
            {
                v.SetInputAsHandled();
            }
        }
        _placeKeyDown = down;
    }

    public override void _Input(InputEvent @event)
    {
        if (Manager.Instance == null)
            return;

        if (@event is InputEventKey { Pressed: true, Echo: false } k)
        {
            if (k.Keycode == Key.Escape || k.PhysicalKeycode == Key.Escape)
            {
                _selectedIndex = null;
                GD.Print("[BuildPlacer] Build cancelled");
                GetViewport().SetInputAsHandled();
                return;
            }

            for (var n = 0; n < s_buildOptions.Length; n++)
            {
                if (MatchesBuildHotkey(k, n) || MatchesDigitUnicode(k, n))
                {
                    if (_scenes == null || _scenes[n] == null)
                    {
                        GD.PrintErr("[BuildPlacer] Scene not loaded for slot " + (n + 1));
                        return;
                    }
                    _selectedIndex = n;
                    var cost = s_buildOptions[n].Cost;
                    GD.Print(
                        $"[BuildPlacer] Slot {n + 1} (cost {cost}) — press {PlaceConfirmKey} to place, Esc cancel"
                    );
                    GetViewport().SetInputAsHandled();
                    return;
                }
            }
        }

        if (
            _selectedIndex == null
            || @event is not InputEventMouseButton { Pressed: true } mb
            || mb.ButtonIndex != PlaceButton
        )
            return;

        if (!TryPlaceAtScreen(mb.Position, out var c) || !c)
            return;
        GetViewport().SetInputAsHandled();
    }

    static bool MatchesDigitUnicode(InputEventKey k, int slot0to4)
    {
        if (k.Unicode is 0u)
            return false;
        var u = (char)k.Unicode;
        var expect = (char)('1' + slot0to4);
        return u == expect;
    }

    static bool MatchesBuildHotkey(InputEventKey k, int slot0to4)
    {
        var a = s_digitKeys[slot0to4];
        var b = s_digitKeys[slot0to4 + 5];
        return k.Keycode == a || k.PhysicalKeycode == a || k.Keycode == b || k.PhysicalKeycode == b;
    }

    bool TryPlaceAtScreen(Vector2 screenPos, out bool consumeEvent)
    {
        consumeEvent = false;
        ResolveMainCamera();
        if (
            MainCamera == null
            || !GodotObject.IsInstanceValid(MainCamera)
            || _selectedIndex is not { } idx
        )
            return false;
        if (Manager.Instance == null || _scenes == null)
            return false;

        consumeEvent = true;
        if (!TryGroundPosition(screenPos, out var worldPos))
        {
            GD.PrintErr("[BuildPlacer] No ground under cursor; aim at the map.");
            return true;
        }

        var data = Manager.Instance.Data;
        var cost = s_buildOptions[idx].Cost;
        if (!data.TrySpend(cost))
        {
            GD.Print($"[BuildPlacer] Not enough (need {cost}, have {data.Currency})");
            return true;
        }

        var scene = _scenes[idx]!;
        var node = scene.Instantiate<Node>();
        if (node == null)
        {
            data.GrantCurrency(cost);
            return true;
        }

        var parent = GetTree().CurrentScene;
        if (parent == null)
        {
            data.GrantCurrency(cost);
            return true;
        }

        if (node is not Node3D n3d)
        {
            parent.AddChild(node);
        }
        else
        {
            // Local Y from packed scene (valid before the node is in the tree; GlobalPosition is not).
            var localY = n3d.Position.Y;
            parent.AddChild(node);
            n3d.GlobalPosition = new Vector3(worldPos.X, worldPos.Y + localY, worldPos.Z);
        }

        ApplyPlayerFaction(node);
        GD.Print("[BuildPlacer] Placed " + node.Name);
        return true;
    }

    bool TryGroundPosition(Vector2 screenPos, out Vector3 worldPos)
    {
        worldPos = default;
        if (MainCamera == null)
            return false;

        if (TryPhysicsRayHit(screenPos, out worldPos))
            return true;
        if (TryPlaneHit(MainCamera, screenPos, GroundPlaneY, out worldPos))
            return true;
        return false;
    }

    bool TryPhysicsRayHit(Vector2 screenPos, out Vector3 hitPos)
    {
        hitPos = default;
        var from = MainCamera!.ProjectRayOrigin(screenPos);
        var to = from + MainCamera.ProjectRayNormal(screenPos) * RaycastLength;
        var space = MainCamera.GetWorld3D().DirectSpaceState;
        var q = PhysicsRayQueryParameters3D.Create(from, to);
        q.CollisionMask = AllPhysicsLayers;
        var hit = space.IntersectRay(q);
        if (hit == null || hit.Count == 0 || !hit.ContainsKey("position"))
            return false;
        hitPos = (Vector3)hit["position"]!;
        return true;
    }

    static bool TryPlaneHit(Camera3D cam, Vector2 screenPos, float planeY, out Vector3 worldPos)
    {
        worldPos = default;
        var from = cam.ProjectRayOrigin(screenPos);
        var dir = cam.ProjectRayNormal(screenPos);
        if (Mathf.Abs(dir.Y) < 0.0001f)
            return false;
        var t = (planeY - from.Y) / dir.Y;
        if (t < 0f)
            return false;
        worldPos = from + dir * t;
        return true;
    }

    void ApplyPlayerFaction(Node node)
    {
        if (node is UnitBase ub)
            ub.FactionID = PlayerFactionId;
        if (node is LettuceGenerator lg)
            lg.FactionID = PlayerFactionId;
    }
}
