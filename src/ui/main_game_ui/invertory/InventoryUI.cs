using Godot;
using Terraria3D;

public partial class InventoryUI : GridContainer
{
    [Export]
    PackedScene ItemGridScene;

    /// <summary>Reference to the player whose inventory is displayed.</summary>
    [Export]
    public Player Player { get; set; }

    private ItemGrid[] _grids = new ItemGrid[Inventory.MainSlots];

    public override void _Ready()
    {
        for (int slot = 0; slot < Inventory.MainSlots; ++slot)
        {
            int row = slot / Inventory.ColSize;
            int col = slot % Inventory.ColSize;
            var grid = ItemGridScene.Instantiate<ItemGrid>();
            grid.Init(row == 0, new Vector2I(row, col), slot);
            _grids[slot] = grid;
            AddChild(grid);
        }
    }

    public override void _Process(double delta)
    {
        if (Player?.Inventory != null)
            RefreshAll();
    }

    /// <summary>Refresh every grid cell from the player's inventory data.</summary>
    public void RefreshAll()
    {
        var inv = Player.Inventory;
        for (int i = 0; i < inv.Main.Length && i < _grids.Length; ++i)
        {
            _grids[i].Refresh(inv.Main[i]);
            _grids[i].SetSelected(i == inv.SelectedIndex);
        }
    }
}
