using System;
using System.Linq;

namespace Terraria3D.items;

/// <summary>
/// Player equipment slots — armor, accessories, vanity, and special equipment.
/// Follows Terraria's layout: 3 armor, 5 accessories, 3 vanity armor,
/// 5 vanity accessories, 5 equipment (pet/light/minecart/mount/hook).
/// </summary>
public class Equipment
{
    // ── Armor slots (3) ───────────────────────────────────────────────

    public ItemStack HeadArmor { get; set; } = ItemStack.Empty;
    public ItemStack BodyArmor { get; set; } = ItemStack.Empty;
    public ItemStack LegsArmor { get; set; } = ItemStack.Empty;

    // ── Accessory slots (5 base, expandable via Demon Heart) ──────────

    public ItemStack[] Accessories { get; }
    private const int AccessorySlotCount = 5;

    // ── Vanity / Social slots (visual override, no stats) ─────────────

    public ItemStack[] VanityArmor { get; }
    public ItemStack[] VanityAccessories { get; }
    private const int VanityArmorCount = 3;
    private const int VanityAccessoryCount = 5;

    // ── Equipment slots (5 special) ───────────────────────────────────

    public ItemStack Pet { get; set; } = ItemStack.Empty;
    public ItemStack LightPet { get; set; } = ItemStack.Empty;
    public ItemStack Minecart { get; set; } = ItemStack.Empty;
    public ItemStack Mount { get; set; } = ItemStack.Empty;
    public ItemStack Hook { get; set; } = ItemStack.Empty;

    // ── Constructor ───────────────────────────────────────────────────

    public Equipment()
    {
        Accessories = new ItemStack[AccessorySlotCount];
        VanityArmor = new ItemStack[VanityArmorCount];
        VanityAccessories = new ItemStack[VanityAccessoryCount];

        for (int i = 0; i < AccessorySlotCount; i++) Accessories[i] = ItemStack.Empty;
        for (int i = 0; i < VanityArmorCount; i++) VanityArmor[i] = ItemStack.Empty;
        for (int i = 0; i < VanityAccessoryCount; i++) VanityAccessories[i] = ItemStack.Empty;
    }

    // ── Computed stats ────────────────────────────────────────────────

    /// <summary>Total defense from all equipped armor and accessories.</summary>
    public int TotalDefense => ComputeDefense();

    /// <summary>Cumulative damage bonus from equipment.</summary>
    public float TotalDamageBonus => ComputeBonus(i => i.DamageBonus);

    /// <summary>Cumulative speed bonus from equipment.</summary>
    public float TotalSpeedBonus => ComputeBonus(i => i.SpeedBonus);

    /// <summary>Extra health from equipment.</summary>
    public int TotalHealthBonus => ComputeBonus(i => i.HealthBonus);

    /// <summary>Extra mana from equipment.</summary>
    public int TotalManaBonus => ComputeBonus(i => i.ManaBonus);

    private int ComputeDefense()
    {
        int defense = 0;
        defense += StatOrZero(HeadArmor, i => i.Defense);
        defense += StatOrZero(BodyArmor, i => i.Defense);
        defense += StatOrZero(LegsArmor, i => i.Defense);
        foreach (var acc in Accessories)
            defense += StatOrZero(acc, i => i.Defense);
        return defense;
    }

    private int ComputeBonus(Func<ItemType, int> selector)
    {
        int total = 0;
        total += StatOrZero(HeadArmor, selector);
        total += StatOrZero(BodyArmor, selector);
        total += StatOrZero(LegsArmor, selector);
        foreach (var acc in Accessories)
            total += StatOrZero(acc, selector);
        return total;
    }

    private float ComputeBonus(Func<ItemType, float> selector)
    {
        float total = 0f;
        total += StatOrZeroF(HeadArmor, selector);
        total += StatOrZeroF(BodyArmor, selector);
        total += StatOrZeroF(LegsArmor, selector);
        foreach (var acc in Accessories)
            total += StatOrZeroF(acc, selector);
        return total;
    }

    private static int StatOrZero(ItemStack? stack, Func<ItemType, int> selector)
    {
        if (stack == null || stack.IsEmpty) return 0;
        var type = stack.Type;
        return type != null ? selector(type) : 0;
    }

    private static float StatOrZeroF(ItemStack? stack, Func<ItemType, float> selector)
    {
        if (stack == null || stack.IsEmpty) return 0f;
        var type = stack.Type;
        return type != null ? selector(type) : 0f;
    }
}
