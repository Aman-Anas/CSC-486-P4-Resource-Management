using Game;
using Godot;

namespace Game.Entities;

/// <summary> Top-right readout: round and currency, synced from <see cref="GameData"/>. </summary>
[GlobalClass]
public partial class BattlefieldEconomyHud : RichTextLabel
{
    public override void _Ready()
    {
        BbcodeEnabled = true;
        FitContent = true;
        ScrollActive = false;
        AutowrapMode = TextServer.AutowrapMode.WordSmart;
        HorizontalAlignment = HorizontalAlignment.Right;
        AddThemeFontSizeOverride("font_size", 28);
        AddThemeFontSizeOverride("bold_font_size", 30);
    }

    public override void _Process(double delta)
    {
        if (Manager.Instance?.Data is not { } d)
            return;
        Text =
            $"[color=#b8c8d8]Round[/color] [b]{d.CurrentRound}[/b]   [color=#b8c8d8]$[/color] [color=#ffd166][b]{d.Currency}[/b][/color]";
    }
}
