using Godot;
using System;

public partial class Bar : Control
{
    public Item[] hotbarItem;
    public Item[,] inventoryItem;

    public void SetHotbarItem(Item[] item)
    {
        hotbarItem = item;
        HBoxContainer hotbar = this.GetNode<HBoxContainer>("HotBar");
        for (int i = 0; i < 9; ++i)
        {
            if (hotbarItem[i] == null)
            {
                continue;
            }
            TextureRect texture = hotbarItem[i].icon.Duplicate() as TextureRect;
            hotbar.GetChild<Panel>(i).AddChild(texture);
        }
    }

    public void SetInventoryItem(Item[,] item)
    {
        inventoryItem = item;
        GridContainer inventory = this.GetNode<GridContainer>("Inventory");
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 9; ++j)
            {
                if (inventoryItem[i, j] == null)
                {
                    continue;
                }
                TextureRect texture = inventoryItem[i, j].icon.Duplicate() as TextureRect;
                inventory.GetChild<Panel>(i * 9 + j).AddChild(texture);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("escape"))
        {
            GridContainer inventory = this.GetNode<GridContainer>("Inventory");
            inventory.Visible = !inventory.Visible;
        }
    }
}
