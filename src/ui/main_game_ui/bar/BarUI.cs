using Godot;

namespace Terraria3D.ui.main_game_ui.bar;

public partial class BarUI : Control
{
    public Bar bar;

    public HBoxContainer hotbar;
    public GridContainer inventory;

    public override void _Ready()
    {
        base._Ready();
        hotbar = this.GetNode<HBoxContainer>("HotBar");
        inventory = this.GetNode<GridContainer>("Inventory");
    }


    public void DrawAll()
    {
        DrawHotbarItem();
        DrawInventoryItem();
        DrawHighlight();
    }

    public void DrawHighlight()
    {
        foreach (var child in hotbar.GetChildren())
        {
            (child.GetNode("Highlight") as Panel).Visible = false;
        }
        (hotbar.GetChild<Panel>(bar.CurrentHotbarId).GetNode("Highlight") as Panel).Visible = true;
    }

    public void DrawHotbarItem()
    {
        for (int i = 0; i < Bar.ColSize; ++i)
        {
            if (bar.HotbarItem[i] == null)
            {
                continue;
            }
            TextureRect texture = bar.HotbarItem[i].Icon.Duplicate() as TextureRect;
            foreach (var child in hotbar.GetChild<Panel>(i).GetChildren())
            {
                if (child is TextureRect)
                {
                    child.QueueFree();
                }

            }
            hotbar.GetChild<Panel>(i).AddChild(texture);
        }
    }

    public void DrawInventoryItem()
    {
        for (int i = 0; i < Bar.RowSize; ++i)
        {
            for (int j = 0; j < Bar.ColSize; ++j)
            {
                if (bar.InventoryItem[i, j] == null)
                {
                    continue;
                }
                TextureRect texture = bar.InventoryItem[i, j].Icon.Duplicate() as TextureRect;
                foreach (var child in inventory.GetChild<Panel>(i).GetChildren())
                {
                    if (child is TextureRect)
                    {
                        child.QueueFree();
                    }
                }
                inventory.GetChild<Panel>(i * Bar.ColSize + j).AddChild(texture);
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