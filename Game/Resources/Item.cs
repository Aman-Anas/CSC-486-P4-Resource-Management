using System;
using Godot;

[GlobalClass]
public partial class Item : Resource
{
    [Export]
    public string Name { get; set; } = "";

    [Export]
    public Color ItemColor { get; set; } = Color.FromHsv(1, 1, 1);

    [Export]
    public Texture2D? Icon { get; set; }
}
