using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entities;
using Godot;

public partial class KeyDoor : Node3D
{
    [Export]
    public Item RequiredKey { get; private set; } = null!;

    [Export]
    CsgBox3D DoorMesh = null!;

    [Export]
    AnimationPlayer animPlayer = null!;

    [Export]
    StringName OpenAnimName = "open";

    public bool Opened { get; set; } = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var mat = (StandardMaterial3D)this.DoorMesh.Material;

        mat.AlbedoColor = RequiredKey.ItemColor;
    }

    //public void Open(HashSet<string> currentKeys)
    public void Open(Inventory inventory)
    {
        if (Opened)
            return;

        if (inventory.HasItem(RequiredKey))
        //if (currentKeys.Contains(Info.LockID))
        {
            animPlayer.Play(OpenAnimName);
            Opened = true;
            inventory.RemoveItem(RequiredKey);
        }
    }
}
