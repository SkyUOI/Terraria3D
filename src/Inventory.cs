using System;
using System.Linq;
using Terraria3D.items;

namespace Terraria3D;

/// <summary>
/// Player inventory — 50 main slots (5 rows × 10 columns), plus dedicated
/// coin and ammo slots. The top row (indices 0–9) is the hotbar.
/// </summary>
public class Inventory
{
    // ── Grid constants (used by InventoryUI) ──────────────────────────

    public const int RowSize = 5;
    public const int ColSize = 10;
    public const int HotbarSize = ColSize; // 10
    public const int MainSlots = RowSize * ColSize; // 50

    // ── Slot arrays ───────────────────────────────────────────────────

    /// <summary>Main inventory slots (0–9 = hotbar, 10–49 = storage).</summary>
    public ItemStack[] Main { get; }

    /// <summary>Coin slots: 0=Copper, 1=Silver, 2=Gold, 3=Platinum.</summary>
    public ItemStack[] Coins { get; }

    /// <summary>Ammo slots (4 dedicated).</summary>
    public ItemStack[] Ammo { get; }

    // ── Hotbar selection ──────────────────────────────────────────────

    /// <summary>Currently selected hotbar slot (0–9).</summary>
    public int SelectedIndex { get; set; }

    /// <summary>Get the currently selected item stack (hotbar only).</summary>
    public ItemStack? SelectedItem =>
        SelectedIndex >= 0 && SelectedIndex < HotbarSize ? Main[SelectedIndex] : null;

    // ── State ─────────────────────────────────────────────────────────

    /// <summary>Whether every main slot is occupied.</summary>
    public bool IsFull => Main.All(s => !s.IsEmpty);

    // ── Constructor ───────────────────────────────────────────────────

    public Inventory()
    {
        Main = new ItemStack[MainSlots];
        Coins = new ItemStack[4];
        Ammo = new ItemStack[4];

        for (int i = 0; i < MainSlots; i++) Main[i] = ItemStack.Empty;
        for (int i = 0; i < 4; i++) { Coins[i] = ItemStack.Empty; Ammo[i] = ItemStack.Empty; }
    }

    // ── Operations ────────────────────────────────────────────────────

    /// <summary>
    /// Add items to the inventory. Tries to merge into existing stacks first,
    /// then fills empty slots. Returns the number of items that couldn't fit.
    /// </summary>
    public int AddItem(string itemId, int amount)
    {
        if (amount <= 0) return 0;
        if (!ItemRegistry.Exists(itemId)) return amount;

        int remaining = amount;

        // 1. Try merging into existing stacks of the same type
        foreach (var stack in Main)
        {
            if (!stack.IsEmpty && stack.ItemId == itemId)
            {
                int capacity = stack.MaxStack - stack.Amount;
                int add = Math.Min(capacity, remaining);
                stack.Amount += add;
                remaining -= add;
                if (remaining <= 0) return 0;
            }
        }

        // 2. Fill empty slots
        foreach (var stack in Main)
        {
            if (stack.IsEmpty)
            {
                int maxStack = ItemRegistry.Get(itemId).MaxStack;
                int add = Math.Min(maxStack, remaining);
                stack.ItemId = itemId;
                stack.Amount = add;
                remaining -= add;
                if (remaining <= 0) return 0;
            }
        }

        return remaining;
    }

    /// <summary>Remove items from a specific slot.</summary>
    public void RemoveItem(int slotIndex, int amount)
    {
        if (slotIndex < 0 || slotIndex >= MainSlots) return;
        var stack = Main[slotIndex];
        if (stack.IsEmpty) return;

        stack.Amount -= amount;
        if (stack.Amount <= 0)
            stack.Clear();
    }

    /// <summary>Swap or merge the contents of two main inventory slots.</summary>
    public void SwapSlots(int from, int to)
    {
        if (from < 0 || from >= MainSlots || to < 0 || to >= MainSlots) return;

        var a = Main[from];
        var b = Main[to];

        // If stacks can merge, merge instead of swapping
        if (a.CanMerge(b))
        {
            a.Merge(b);
            return;
        }
        if (b.CanMerge(a))
        {
            b.Merge(a);
            return;
        }

        // Otherwise, swap
        (Main[from], Main[to]) = (b, a);
    }

    /// <summary>Check if the inventory contains at least <paramref name="amount"/> of an item.</summary>
    public bool HasItem(string itemId, int amount = 1)
    {
        int found = 0;
        foreach (var stack in Main)
        {
            if (!stack.IsEmpty && stack.ItemId == itemId)
                found += stack.Amount;
        }
        return found >= amount;
    }

    /// <summary>Count total items of a given type across all main slots.</summary>
    public int CountItem(string itemId)
    {
        int total = 0;
        foreach (var stack in Main)
        {
            if (!stack.IsEmpty && stack.ItemId == itemId)
                total += stack.Amount;
        }
        return total;
    }
}
