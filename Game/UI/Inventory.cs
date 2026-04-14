using System.Collections.Generic;
using Godot;

public partial class Inventory : Control
{
    private class InventoryItem(Item item, HotbarSlot slot)
    {
        public Item Item { get; set; } = item;
        public int Count { get; set; }
        public int Index { get; set; }
        public HotbarSlot Slot { get; set; } = slot;
    }

    private readonly Dictionary<string, InventoryItem> contents = [];

    [Export]
    public PackedScene slotScene = null!;

    [Export]
    public HBoxContainer hotbarContainer = null!;

    // clear just inventory items
    public void ClearInventory()
    {
        contents.Clear();
        RefreshHotbar();
    }

    // Clear and regenerate hotbar slots
    public void RefreshHotbar()
    {
        foreach (Node child in hotbarContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var inventoryItem in contents.Values)
        {
            // Instantiate a new slot for each inventory item
            var slot = slotScene.Instantiate<HotbarSlot>();
            inventoryItem.Slot = slot;

            // Update the slot's texture based on the item's tex
            slot.SetItem(inventoryItem.Item);
            hotbarContainer.AddChild(slot);
        }
    }

    public override void _Ready()
    {
        RefreshHotbar();
    }

    public void AddItem(Item item)
    {
        if (item == null)
            return;
        if (contents.TryGetValue(item.Name, out var inventoryItem))
        {
            inventoryItem.Count++;

            // update count in hotbar
            // with keys and statues, this probably won't ever be called
            // since there's only ever a single key or statue of a type, i think
        }
        else
        {
            HotbarSlot slot = slotScene.Instantiate<HotbarSlot>();

            var newItem = new InventoryItem(item, slot) { Count = 1, Index = contents.Count };

            contents[item.Name] = newItem;

            // add icon to hotbar
            slot.SetItem(item);
            //GD.Print(slot);
            hotbarContainer.AddChild(slot);
        }

        GD.Print($"[Inventory] - Picked up: {item.Name}");
        RefreshHotbar();
    }

    public void RemoveItem(Item item)
    {
        if (item == null)
            return;
        contents.Remove(item.Name);
        RefreshHotbar();
    }

    // public void RemoveItem(Item item)
    // {
    //     if (item == null)
    //         return;
    //     if (!_lookup.TryGetValue(item, out var inventoryItem))
    //         return;

    //     inventoryItem.Count--;
    //     if (inventoryItem.Count <= 0)
    //     {
    //         _lookup.Remove(item);
    //         _contents.Remove(inventoryItem);
    //         inventoryItem.Slot.QueueFree();

    //         for (int i = 0; i < _contents.Count; i++)
    //         {
    //             _contents[i].Index = i;
    //         }
    //     }
    // }

    // public void RemoveItemByResource(Resource resource)
    // {
    //     if (resource == null)
    //         return;
    //     if (resource is Item item)
    //     {
    //         RemoveItem(item);
    //     }
    // }

    public bool HasItem(Item item)
    {
        return item != null && contents.ContainsKey(item.Name);
    }

    // public bool HasItemByResource(Resource resource)
    // {
    //     return resource != null && resource is Item item && _lookup.ContainsKey(item);
    // }
}
