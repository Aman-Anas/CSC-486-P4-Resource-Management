using System;
using Godot;

public partial class HotbarSlot : Control
{
    [Export]
    public TextureRect icon = null!;

    public void SetItem(Item item)
    {
        icon.Texture = item?.Icon;
    }
}
