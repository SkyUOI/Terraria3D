using Godot;
using System;

public partial class Item : Node
{
    [Export]
    public Sprite2D sprite;

}

public enum ItemId
{
    IronPickaxe = 1,
}
