using Game;
using Godot;

namespace Game.Entities;

/// <summary>
/// Applies round-end income to <see cref="GameData"/>. In v1, press a key to simulate round end
/// until a real wave/round system calls the same API.
/// </summary>
[GlobalClass]
public partial class BattlefieldEconomy : Node
{
    [Export]
    public Key EndRoundKey { get; set; } = Key.Key9;

    public override void _Ready()
    {
        if (Manager.Instance?.Data is { } data)
        {
            if (data.Currency < 0)
                data.Currency = 0;
            if (data.CurrentRound < 1)
                data.CurrentRound = 1;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed)
            return;
        if (key.Keycode != EndRoundKey && key.PhysicalKeycode != EndRoundKey)
            return;
        if (Manager.Instance == null)
            return;

        var data = Manager.Instance.Data;
        var earned = data.ComputeRoundEndIncome();
        data.ApplyRoundEndIncomeAndAdvance();
        GD.Print(
            $"[BattlefieldEconomy] Round end: +{earned}  ->  $={data.Currency}  (now round {data.CurrentRound})"
        );
        GetViewport().SetInputAsHandled();
    }
}
