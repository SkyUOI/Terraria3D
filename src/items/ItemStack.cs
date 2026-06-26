using System;

namespace Terraria3D.items;

/// <summary>
/// Represents a stack of items occupying a single inventory or equipment slot.
/// An empty slot has <c>ItemId = null</c> or <c>Amount = 0</c>.
/// </summary>
public class ItemStack
{
    /// <summary>Item type identifier (matches <see cref="ItemRegistry"/> key). Null/empty = empty slot.</summary>
    public string? ItemId { get; set; }

    /// <summary>Number of items in this stack.</summary>
    public int Amount { get; set; }

    /// <summary>Whether this slot is empty.</summary>
    public bool IsEmpty => string.IsNullOrEmpty(ItemId) || Amount <= 0;

    /// <summary>The item type metadata, or null if empty.</summary>
    public ItemType? Type => !IsEmpty && ItemRegistry.Exists(ItemId!)
        ? ItemRegistry.Get(ItemId!)
        : null;

    /// <summary>Maximum stack size for the current item type.</summary>
    public int MaxStack => Type?.MaxStack ?? 0;

    /// <summary>A shared empty stack instance.</summary>
    public static ItemStack Empty => new() { ItemId = null, Amount = 0 };

    /// <summary>Create a new item stack.</summary>
    public ItemStack() { }

    /// <summary>Create a new item stack with the given type and amount.</summary>
    public ItemStack(string itemId, int amount = 1)
    {
        ItemId = itemId;
        Amount = amount;
    }

    /// <summary>
    /// Whether <paramref name="other"/> can be merged into this stack
    /// (same item type and combined total within max stack size).
    /// </summary>
    public bool CanMerge(ItemStack other)
    {
        if (IsEmpty || other.IsEmpty) return false;
        if (ItemId != other.ItemId) return false;
        return Amount + other.Amount <= MaxStack;
    }

    /// <summary>
    /// Merge <paramref name="other"/> into this stack. The other stack
    /// will be emptied or reduced. Call <see cref="CanMerge"/> first.
    /// </summary>
    public void Merge(ItemStack other)
    {
        int capacity = MaxStack - Amount;
        int transfer = Math.Min(capacity, other.Amount);
        Amount += transfer;
        other.Amount -= transfer;
        if (other.Amount <= 0)
        {
            other.ItemId = null;
            other.Amount = 0;
        }
    }

    /// <summary>
    /// Split <paramref name="count"/> items from this stack into a new stack.
    /// Returns null if the stack doesn't have enough items.
    /// </summary>
    public ItemStack? Split(int count)
    {
        if (count <= 0 || count > Amount) return null;
        Amount -= count;
        var split = new ItemStack(ItemId!, count);
        if (Amount <= 0)
        {
            ItemId = null;
            Amount = 0;
        }
        return split;
    }

    /// <summary>Make this slot empty.</summary>
    public void Clear()
    {
        ItemId = null;
        Amount = 0;
    }
}
