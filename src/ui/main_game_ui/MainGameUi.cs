using Godot;

namespace Terraria3D.ui.main_game_ui;

public partial class MainGameUi : Control
{
    [Export]
    public Player Player { get; set; }
    [Export]
    public InventoryUI InventoryUI { get; set; }
    [Export]
    public hearts.Hearts Hearts { get; set; }
    [Export]
    public stars.Stars Stars { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Player != null)
        {
            Hearts.SetHp(Player.Health);
            Hearts.SetHpMax(Player.EffectiveMaxHealth);
            Stars.SetMp(Player.Mana);
            Stars.SetMpMax(Player.EffectiveMaxMana);
        }
        else
        {
            Hearts.SetHp(100);
            Hearts.SetHpMax(460);
            Stars.SetMp(50);
            Stars.SetMpMax(200);
        }
    }

    public bool PointInUi(Vector2 point)
    {
        return Hearts.GetGlobalRect().HasPoint(point) || Stars.GetGlobalRect().HasPoint(point);
    }
}
