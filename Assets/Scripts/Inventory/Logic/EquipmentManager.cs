using System;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Data;

namespace Inventory.Logic
{
    public class EquipmentManager : SingletonMono<EquipmentManager>
    {
        // ───────── 런타임 데이터 ─────────
        private Dictionary<ArmorSlotType, ItemStack> _equippedArmors;
        private ItemStack _equippedWeapon;

        // ───────── 읽기 전용 접근 ─────────
        public IReadOnlyDictionary<ArmorSlotType, ItemStack> EquippedArmors => _equippedArmors;
        public ItemStack EquippedWeapon => _equippedWeapon;

        // ───────── 이벤트 (UI 구독용) ─────────
        /// <summary>장비 슬롯이 바뀔 때마다 발행.</summary>
        public event Action OnEquipmentChanged;

        // ──────────────────────────────────────
        //  초기화
        // ──────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();

            _equippedArmors = new Dictionary<ArmorSlotType, ItemStack>();
            foreach (ArmorSlotType slot in Enum.GetValues(typeof(ArmorSlotType)))
            {
                _equippedArmors[slot] = null;
            }
        }

        // ──────────────────────────────────────
        //  장착
        // ──────────────────────────────────────
        /// <summary>
        /// 아이템을 장착한다.
        /// ItemType에 따라 무기/방어구 슬롯을 자동 판별한다.
        /// </summary>
        /// <returns>기존에 장착되어 있던 아이템 (없으면 null).</returns>
        public ItemStack Equip(ItemStack item)
        {
            if (item == null || item.data == null)
                return null;

            switch (item.data.itemType)
            {
                case ItemType.Weapon:
                    return EquipWeapon(item);

                case ItemType.Armor:
                    return EquipArmor(item);

                default:
                    Debug.LogWarning($"[EquipmentManager] 장착 불가 아이템 타입: {item.data.itemType}");
                    return item; // 장착 실패 → 원본 반환
            }
        }

        private ItemStack EquipWeapon(ItemStack item)
        {
            var previous = _equippedWeapon;
            _equippedWeapon = item;
            OnEquipmentChanged?.Invoke();
            return previous;
        }

        private ItemStack EquipArmor(ItemStack item)
        {
            var armorData = item.data as ArmorData;
            if (armorData == null)
                return item;

            var slot = armorData.slotType;
            var previous = _equippedArmors[slot];
            _equippedArmors[slot] = item;
            OnEquipmentChanged?.Invoke();
            return previous;
        }

        // ──────────────────────────────────────
        //  해제
        // ──────────────────────────────────────
        /// <summary>
        /// 방어구 슬롯을 해제한다.
        /// 인벤토리에 빈 칸이 있는지는 호출자(InventoryManager)가 판단한다.
        /// </summary>
        /// <returns>해제된 아이템 (비어 있으면 null).</returns>
        public ItemStack Unequip(ArmorSlotType slot)
        {
            var previous = _equippedArmors[slot];
            if (previous == null)
                return null;

            _equippedArmors[slot] = null;
            OnEquipmentChanged?.Invoke();
            return previous;
        }

        /// <summary>무기 슬롯을 해제한다.</summary>
        public ItemStack UnequipWeapon()
        {
            var previous = _equippedWeapon;
            if (previous == null)
                return null;

            _equippedWeapon = null;
            OnEquipmentChanged?.Invoke();
            return previous;
        }

        /// <summary>
        /// 특정 방어구 슬롯을 해제하고 인벤토리로 돌려보낸다.
        /// 인벤토리 공간이 부족하면 해제하지 않고 false를 반환한다.
        /// </summary>
        public bool UnequipToInventory(ArmorSlotType slot)
        {
            var item = _equippedArmors[slot];
            if (item == null)
                return true; // 이미 비어 있음

            if (!InventoryManager.Instance.HasEmptySlot)
                return false;

            _equippedArmors[slot] = null;
            InventoryManager.Instance.AddItem(item.data, item.currentStack);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        /// <summary>무기를 해제하고 인벤토리로 돌려보낸다.</summary>
        public bool UnequipWeaponToInventory()
        {
            if (_equippedWeapon == null)
                return true;

            if (!InventoryManager.Instance.HasEmptySlot)
                return false;

            var weapon = _equippedWeapon;
            _equippedWeapon = null;
            InventoryManager.Instance.AddItem(weapon.data, weapon.currentStack);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        // ──────────────────────────────────────
        //  조회
        // ──────────────────────────────────────
        /// <summary>특정 방어구 슬롯에 장착된 아이템을 반환.</summary>
        public ItemStack GetEquippedArmor(ArmorSlotType slot)
        {
            return _equippedArmors.TryGetValue(slot, out var item) ? item : null;
        }

        /// <summary>특정 슬롯에 장비가 장착되어 있는지.</summary>
        public bool IsSlotOccupied(ArmorSlotType slot)
        {
            return _equippedArmors.TryGetValue(slot, out var item) && item != null;
        }

        public bool IsWeaponEquipped => _equippedWeapon != null;
    }
}
