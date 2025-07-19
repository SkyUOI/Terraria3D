using Godot;

namespace Terraria3D.ui.main_game_ui;

public partial class MainGameUi : Control
{
    [Export]
    public Player Player;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Player != null)
        {
            GetNode("Hearts").Call("set_hp", Player.Health);
            GetNode("Hearts").Call("set_hp_max", Player.HealthMax);
            GetNode("Stars").Call("set_mp", Player.Mana);
            GetNode("Stars").Call("set_mp_max", Player.ManaMax);
        }
        else
        {
            GetNode("Hearts").Call("set_hp", 100);
            GetNode("Hearts").Call("set_hp_max", 460);
            GetNode("Stars").Call("set_mp", 50);
            GetNode("Stars").Call("set_mp_max", 200);
            var ironPickaxe = GD.Load<PackedScene>("res://src/item/IronPickaxe/iron_pickaxe.tscn").Instantiate<item.Item>();
            var bar = GetNode<bar.Bar>("Bar");
            bar.SetHotbarItem([ironPickaxe, null, null, null, null, null, null, null, null]);
            bar.SetInventoryItem(new[,] {
                {null, null, null, null, null, null, null, null, null},
                {null, null, ironPickaxe, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null, null},
            });
        }
    }

    public bool PointInUi(Vector2 point)
    {
        return GetNode<Control>("Hearts").GetGlobalRect().HasPoint(point) || GetNode<Control>("Stars").GetGlobalRect().HasPoint(point);
    }
}