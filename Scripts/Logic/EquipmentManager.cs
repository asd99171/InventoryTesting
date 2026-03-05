using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    public event Action OnEquipmentChanged;

    private Dictionary<ArmorSlotType, ItemStack> equippedSlots = new Dictionary<ArmorSlotType, ItemStack>();
    private ItemStack equippedWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public ItemStack Equip(ItemStack item)
    {
        ItemStack previous = null;

        if (item.data is WeaponData)
        {
            previous = equippedWeapon;
            equippedWeapon = item;
        }
        else if (item.data is ArmorData armorData)
        {
            equippedSlots.TryGetValue(armorData.slotType, out previous);
            equippedSlots[armorData.slotType] = item;
        }

        OnEquipmentChanged?.Invoke();
        return previous;
    }

    public ItemStack Unequip(ArmorSlotType slot)
    {
        if (!equippedSlots.TryGetValue(slot, out ItemStack item)) return null;

        equippedSlots.Remove(slot);
        OnEquipmentChanged?.Invoke();
        return item;
    }

    public ItemStack UnequipWeapon()
    {
        ItemStack weapon = equippedWeapon;
        equippedWeapon = null;
        OnEquipmentChanged?.Invoke();
        return weapon;
    }

    public ItemStack GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    public ItemStack GetEquippedArmor(ArmorSlotType slot)
    {
        equippedSlots.TryGetValue(slot, out ItemStack item);
        return item;
    }

    public EquipmentStats GetTotalStats()
    {
        EquipmentStats stats = new EquipmentStats();

        if (equippedWeapon?.data is WeaponData weapon)
        {
            stats.totalAttack += weapon.attackPower;
        }

        foreach (var kvp in equippedSlots)
        {
            if (kvp.Value.data is ArmorData armor)
            {
                stats.totalDefense += armor.defense;
                stats.totalMagicResist += armor.magicResistance;
            }
        }

        return stats;
    }
}
