using Godot;

namespace Terraria3D.ui.main_game_ui.bar;

public partial class Bar : Control
{
    public item.Item[] hotbarItem;
    public item.Item[,] inventoryItem;

    public void SetHotbarItem(Terraria3D.item.Item[] item)
    {
        hotbarItem = item;
        HBoxContainer hotbar = this.GetNode<HBoxContainer>("HotBar");
        for (int i = 0; i < 9; ++i)
        {
            if (hotbarItem[i] == null)
            {
                continue;
            }
            TextureRect texture = hotbarItem[i].Icon.Duplicate() as TextureRect;
            hotbar.GetChild<Panel>(i).AddChild(texture);
        }
    }

    public void SetInventoryItem(Terraria3D.item.Item[,] item)
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
                TextureRect texture = inventoryItem[i, j].Icon.Duplicate() as TextureRect;
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