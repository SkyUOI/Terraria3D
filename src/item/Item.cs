using Godot;

namespace Terraria3D.item;

public enum ItemId
{
    IronPickaxe = 1,
}

public class Item(ItemId itemId, Texture2D icon, int count)
{
    public ItemId Id = itemId;
    public Texture2D Icon = icon;
    // 0 means it can not be stacked
    public int Count = count;
}

public interface IItem
{
    public static abstract ItemId Id { get; }

    public abstract Item NewItem();
}
