using System.Text;
using Game;
using Godot;

namespace Game.Entities;

/// <summary>
/// Below money on the right: 1–6 with unit names; selected key line is yellow, others off-white.
/// </summary>
[GlobalClass]
public partial class BattlefieldBuildHud : RichTextLabel
{
    /// <summary> From this node: up to <see cref="CanvasLayer"/>, then to the scene root sibling <c>BuildPlacer</c> (e.g. <c>../../BuildPlacer</c> when this is under <c>EconomyCanvas</c>). </summary>
    [Export]
    public NodePath BuildPlacerPath { get; set; } = new("../../BuildPlacer");

    /// <summary> Unselected: readable light text (not dim gray). </summary>
    const string LineUnselected = "[color=#e8eaef]{0} - {1}  $ {2}[/color]";

    /// <summary> Active hotkey row. </summary>
    const string LineSelected = "[color=#ffdd44][b]{0} - {1}  $ {2}[/b][/color]";

    /// <summary> Footer: choose + controls (no gray). </summary>
    const string HintNoSelection = "[color=#b3d9ff]1–6: choose  ·  B: place  ·  Esc: cancel[/color]";

    /// <summary> Shown when a slot is armed — same info as <see cref="HintNoSelection"/>, with Esc/B always visible. </summary>
    const string HintWhenArmed = "[color=#b3d9ff]1–6: switch  ·  B: place  ·  Esc: cancel[/color]";

    /// <summary> Unaffordable line — cyan reads clearly on warm brown ~#9e6434. </summary>
    const string CantAffordColor = "#22d3ee";

    BuildPlacer? _placer;

    public override void _Ready()
    {
        BbcodeEnabled = true;
        AutowrapMode = TextServer.AutowrapMode.WordSmart;
        FitContent = true;
        ScrollActive = false;
        HorizontalAlignment = HorizontalAlignment.Right;
        AddThemeFontSizeOverride("font_size", 17);
    }

    public override void _Process(double delta)
    {
        if (Manager.Instance?.Data is not { } d)
        {
            Text = "";
            return;
        }

        if (_placer == null)
        {
            _placer = GetNodeOrNull<BuildPlacer>(BuildPlacerPath);
            if (_placer == null && GetTree()?.CurrentScene is Node root)
                _placer = root.GetNodeOrNull<BuildPlacer>(new NodePath("BuildPlacer"));
        }

        int? selected = _placer != null ? _placer.SelectedBuildIndex : null;

        var sb = new StringBuilder();
        for (var n = 0; n < BuildPlacer.BuildOptionCount; n++)
        {
            var name = BuildPlacer.GetBuildOptionName(n);
            var cost = BuildPlacer.GetBuildOptionCost(n);
            var isSel = selected is { } s && s == n;
            if (n > 0)
                sb.Append('\n');
            if (isSel)
                sb.Append(string.Format(LineSelected, n + 1, name, cost));
            else
                sb.Append(string.Format(LineUnselected, n + 1, name, cost));
        }

        if (selected is not { } i)
            sb.Append('\n').Append(HintNoSelection);
        else
        {
            var cost = BuildPlacer.GetBuildOptionCost(i);
            var can = d.Currency >= cost;
            if (can)
                sb.Append("\n[color=#8ce99a]Ready to build[/color]");
            else
                sb.Append($"\n[color={CantAffordColor}]Need ${cost - d.Currency} more[/color]");
            sb.Append('\n').Append(HintWhenArmed);
        }

        Text = sb.ToString();
    }
}
