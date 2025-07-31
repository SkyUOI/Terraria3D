using Godot;
using System;
using Terraria3D.item;

public partial class Inventory : Node
{
    public static Inventory Instance { get; private set; }
    public const int RowSize = 5;
    public const int ColSize = 10;
    public Item[,] Items { get; set; } = new Item[RowSize, ColSize];
    public int Selected { get; set; } = 0;

    public Item GetSelected() => Items[Selected / ColSize, Selected % ColSize];

    private ItemGrid selectedNode;

    public void Select(ItemGrid itemGrid)
    {
        var idx = itemGrid.Index;
        var newSelected = idx.X * ColSize + idx.Y;
        if (newSelected == Selected)
        {
            return;
        }
        Selected = newSelected;
        if (selectedNode != null)
        {
            selectedNode.Unselect(idx);
        }
        selectedNode = itemGrid;
    }
}
