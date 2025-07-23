using Godot;

namespace Terraria3D.ui.main_game_ui.bar;

public partial class BarUI : Control
{
    public Bar bar;

    public void SetBarItem(Terraria3D.Bar bar)
    {
        this.bar = bar;
    }

    public void SetHotbarItem()
    {
        HBoxContainer hotbar = this.GetNode<HBoxContainer>("HotBar");
        for (int i = 0; i < Bar.ColSize; ++i)
        {
            if (bar.hotbarItem[i] == null)
            {
                continue;
            }
            TextureRect texture = bar.hotbarItem[i].Icon.Duplicate() as TextureRect;
            hotbar.GetChild<Panel>(i).AddChild(texture);
        }
    }

    public void SetInventoryItem()
    {
        GridContainer inventory = this.GetNode<GridContainer>("Inventory");
        for (int i = 0; i < Bar.RowSize; ++i)
        {
            for (int j = 0; j < Bar.ColSize; ++j)
            {
                if (bar.inventoryItem[i, j] == null)
                {
                    continue;
                }
                TextureRect texture = bar.inventoryItem[i, j].Icon.Duplicate() as TextureRect;
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