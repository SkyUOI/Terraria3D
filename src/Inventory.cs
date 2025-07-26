using Godot;
using System;
using Terraria3D.item;

public partial class Inventory
{
    public const int RowSize = 5;
    public const int ColSize = 10;
    public static Item[,] Items { get; set; } = new Item[RowSize, ColSize];
    public static int Selected { get; set; }
}
