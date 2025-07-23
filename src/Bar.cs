using Godot;
using System;

namespace Terraria3D;

public partial class Bar : Node
{
    
    public const int RowSize = 4;
    public const int ColSize = 10;
    public item.Item[] hotbarItem;
    public item.Item[,] inventoryItem;

    public static Bar EmptyBar()
    {
        var bar = new Bar();
        bar.hotbarItem = new item.Item[ColSize];
        bar.inventoryItem = new item.Item[RowSize, ColSize];
        return bar;
    }

    // CurrentHotbarId的0~9代表从左到右的对应物品栏, 与物品栏上面标注的序号并不对应
    public int CurrentHotbarId = 0;

}
