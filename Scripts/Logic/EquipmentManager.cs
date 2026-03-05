using System;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Data;

namespace InventorySystem.Logic
{
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }

        // ── Events (UI 구독용) ──────────────────────────────
        /// <summary>장비 변경 시 발행</summary>
        public event Action OnEquipmentChanged;

        /// <summary>아이템 장착 시 발행 (장착된 아이템)</summary>
        public event Action<ItemStack> OnItemEquipped;

        /// <summary>아이템 해제 시 발행 (해제된 아이템)</summary>
        public event Action<ItemStack> OnItemUnequipped;

        /// <summary>합산 스탯이 갱신될 때 발행</summary>
        public event Action<EquipmentStats> OnStatsUpdated;

        // ── Fields ──────────────────────────────────────────
        private Dictionary<ArmorSlotType, ItemStack> equippedArmors;
        private ItemStack equippedWeapon;

        public IReadOnlyDictionary<ArmorSlotType, ItemStack> EquippedArmors => equippedArmors;
        public ItemStack EquippedWeapon => equippedWeapon;

        // ── Unity Lifecycle ─────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            equippedArmors = new Dictionary<ArmorSlotType, ItemStack>();
            foreach (ArmorSlotType slot in Enum.GetValues(typeof(ArmorSlotType)))
            {
                equippedArmors[slot] = null;
            }
        }

        // ── Public API ──────────────────────────────────────

        /// <summary>
        /// 아이템을 장착한다. 해당 슬롯에 기존 장비가 있으면 반환한다.
        /// </summary>
        public ItemStack Equip(ItemStack item)
        {
            if (item == null || item.data == null) return null;

            switch (item.data.itemType)
            {
                case ItemType.Weapon:
                    return EquipWeapon(item);

                case ItemType.Armor:
                    return EquipArmor(item);

                default:
                    Debug.LogWarning($"[EquipmentManager] 장착 불가능한 아이템 타입: {item.data.itemType}");
                    return item;
            }
        }

        /// <summary>
        /// 지정 방어구 슬롯의 장비를 해제한다. 해제된 아이템을 반환한다.
        /// </summary>
        public ItemStack Unequip(ArmorSlotType slot)
        {
            if (!equippedArmors.ContainsKey(slot) || equippedArmors[slot] == null)
                return null;

            ItemStack removed = equippedArmors[slot];
            equippedArmors[slot] = null;

            OnItemUnequipped?.Invoke(removed);
            NotifyChange();

            return removed;
        }

        /// <summary>
        /// 무기를 해제한다. 해제된 무기를 반환한다.
        /// </summary>
        public ItemStack UnequipWeapon()
        {
            if (equippedWeapon == null) return null;

            ItemStack removed = equippedWeapon;
            equippedWeapon = null;

            OnItemUnequipped?.Invoke(removed);
            NotifyChange();

            return removed;
        }

        /// <summary>
        /// 지정 방어구 슬롯을 해제하고 인벤토리에 반환한다.
        /// </summary>
        public bool UnequipToInventory(ArmorSlotType slot)
        {
            if (InventoryManager.Instance == null) return false;

            ItemStack removed = Unequip(slot);
            if (removed == null) return false;

            return InventoryManager.Instance.AddItem(removed.data, removed.currentStack);
        }

        /// <summary>
        /// 무기를 해제하고 인벤토리에 반환한다.
        /// </summary>
        public bool UnequipWeaponToInventory()
        {
            if (InventoryManager.Instance == null) return false;

            ItemStack removed = UnequipWeapon();
            if (removed == null) return false;

            return InventoryManager.Instance.AddItem(removed.data, removed.currentStack);
        }

        /// <summary>
        /// 장착 중인 모든 장비의 합산 스탯을 계산한다.
        /// </summary>
        public EquipmentStats GetTotalStats()
        {
            var stats = new EquipmentStats();

            // 무기 스탯
            if (equippedWeapon != null && equippedWeapon.data is WeaponData weapon)
            {
                stats.totalAttack += weapon.attackPower;
                stats.attackSpeed = weapon.attackSpeed;
            }

            // 방어구 스탯
            foreach (var kvp in equippedArmors)
            {
                if (kvp.Value != null && kvp.Value.data is ArmorData armor)
                {
                    stats.totalDefense += armor.defense;
                    stats.totalMagicResist += armor.magicResistance;
                }
            }

            return stats;
        }

        /// <summary>
        /// 특정 방어구 슬롯에 장착된 아이템을 반환한다.
        /// </summary>
        public ItemStack GetEquippedArmor(ArmorSlotType slot)
        {
            return equippedArmors.TryGetValue(slot, out var item) ? item : null;
        }

        /// <summary>
        /// 장비 슬롯이 비어있는지 확인한다.
        /// </summary>
        public bool IsSlotEmpty(ArmorSlotType slot)
        {
            return !equippedArmors.ContainsKey(slot) || equippedArmors[slot] == null;
        }

        public bool IsWeaponSlotEmpty()
        {
            return equippedWeapon == null;
        }

        // ── Private Helpers ─────────────────────────────────

        private ItemStack EquipWeapon(ItemStack item)
        {
            ItemStack previous = equippedWeapon;
            equippedWeapon = item;

            OnItemEquipped?.Invoke(item);
            NotifyChange();

            return previous;
        }

        private ItemStack EquipArmor(ItemStack item)
        {
            if (item.data is not ArmorData armorData)
            {
                Debug.LogWarning("[EquipmentManager] Armor 타입이지만 ArmorData가 아닙니다.");
                return item;
            }

            ArmorSlotType slot = armorData.slotType;
            ItemStack previous = equippedArmors[slot];
            equippedArmors[slot] = item;

            OnItemEquipped?.Invoke(item);
            NotifyChange();

            return previous;
        }

        private void NotifyChange()
        {
            OnEquipmentChanged?.Invoke();
            OnStatsUpdated?.Invoke(GetTotalStats());
        }
    }
}
