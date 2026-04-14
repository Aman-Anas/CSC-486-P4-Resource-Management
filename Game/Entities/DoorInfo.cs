using Godot;

namespace Game.Entities;

[GlobalClass]
public partial class DoorInfo : Resource
{
    [Export]
    public Color DoorColor { get; set; } = Color.FromHsv(1, 1, 1);

    [Export]
    public string LockID { get; set; } = "";
}
