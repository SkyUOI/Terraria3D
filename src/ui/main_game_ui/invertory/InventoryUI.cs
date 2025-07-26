using Godot;
using System;

public partial class InventoryUI : GridContainer
{
    [Export]
    PackedScene ItemGridScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        for (int i = 0; i < Inventory.RowSize; ++i)
        {
            for (int j = 0; j < Inventory.ColSize; ++j)
            {
                var grid = ItemGridScene.Instantiate<ItemGrid>();
                grid.Init(i == 0, new Vector2I(i, j));
                AddChild(grid);
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
