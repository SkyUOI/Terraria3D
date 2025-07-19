using Godot;
using System;

public partial class MainGameUi : Control
{
    [Export]
    public Player player;

    override public void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (player != null)
        {
            GetNode("Hearts").Call("set_hp", player.Health);
            GetNode("Hearts").Call("set_hp_max", player.HealthMax);
            GetNode("Stars").Call("set_mp", player.Mana);
            GetNode("Stars").Call("set_mp_max", player.ManaMax);
        }
        else
        {
            GetNode("Hearts").Call("set_hp", 100);
            GetNode("Hearts").Call("set_hp_max", 460);
            GetNode("Stars").Call("set_mp", 50);
            GetNode("Stars").Call("set_mp_max", 200);
            Item ironPickaxe = GD.Load<PackedScene>("res://src/item/IronPickaxe/iron_pickaxe.tscn").Instantiate<Item>();
            Bar bar = GetNode("Bar") as Bar;
            bar.SetHotbarItem([ironPickaxe, null, null, null, null, null, null, null, null]);
            bar.SetInventoryItem(new Item[,] {
                {null, null, null, null, null, null, null, null, null},
                {null, null, ironPickaxe, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null, null},
            });
        }
    }

    public bool PointInUI(Vector2 point)
    {
        return GetNode<Control>("Hearts").GetGlobalRect().HasPoint(point) || GetNode<Control>("Stars").GetGlobalRect().HasPoint(point);
    }
}
