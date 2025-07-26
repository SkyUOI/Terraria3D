using Godot;

namespace Terraria3D.item.IronPickaxe;

public partial class IronPickaxe : Node, IItem
{
    public static ItemId Id => ItemId.IronPickaxe;

    [Export]
    public Texture2D Icon { get; set; }
    public Item NewItem()
    {
        var item = new Item(Id, Icon, 0);
        return item;
    }
}
