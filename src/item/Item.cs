using Godot;

namespace Terraria3D.item;

public partial class Item : Node
{
    [Export]
    public TextureRect Icon;

}

public enum ItemId
{
    IronPickaxe = 1,
}