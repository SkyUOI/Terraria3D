using Godot;
using System;

public partial class Item : Node
{
    [Export]
    public TextureRect icon;

}

public enum ItemId
{
    IronPickaxe = 1,
}
