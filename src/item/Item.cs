using Godot;

namespace Terraria3D.item;

public enum ItemId
{
    IronPickaxe = 1,
}

public class Item
{
    public ItemId Id;
    public Texture2D Icon;
}

public interface IItem
{
    public static abstract ItemId Id { get; }

    public abstract Item NewItem();
}
