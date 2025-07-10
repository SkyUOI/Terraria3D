using Godot;
using System;

public partial class Item : Node
{
    [Export]
    public Sprite2D sprite;

}

enum ItemId
{
    IronPickaxe = 1,
}
