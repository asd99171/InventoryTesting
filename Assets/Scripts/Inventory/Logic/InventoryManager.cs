using System;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Data;

namespace Inventory.Logic
{
    public class InventoryManager : SingletonMono<InventoryManager>
    {
        // ───────── 설정 ─────────
        [SerializeField] private int maxSlotCount = 30;

        // ───────── 런타임 데이터 ─────────
        private List<ItemStack> _slots;
        public IReadOnlyList<ItemStack> Slots => _slots;
        public int MaxSlotCount => maxSlotCount;

        // ───────── 이벤트 (UI 구독용) ─────────
        /// <summary>슬롯 내용이 바뀔 때마다 발행.</summary>
        public event Action OnInventoryChanged;

        // ──────────────────────────────────────
        //  초기화
        // ──────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            _slots = new List<ItemStack>(maxSlotCount);
        }

        // ──────────────────────────────────────
        //  아이템 추가
        // ──────────────────────────────────────
        /// <summary>
        /// 인벤토리에 아이템을 추가한다.
        /// 스택 가능한 기존 슬롯을 먼저 채우고, 나머지는 빈 슬롯에 넣는다.
        /// </summary>
        /// <returns>전부 수용하면 true, 공간 부족 시 false (가능한 만큼은 추가됨).</returns>
        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            int remaining = amount;

            // 1) 기존 스택에 채우기 (스택 가능 아이템만)
            if (item.maxStackCount > 1)
            {
                for (int i = 0; i < _slots.Count && remaining > 0; i++)
                {
                    if (_slots[i].data.itemID == item.itemID && !_slots[i].IsStackFull())
                    {
                        remaining = _slots[i].AddStack(remaining);
                    }
                }
            }

            // 2) 빈 슬롯에 새 ItemStack 생성
            while (remaining > 0 && _slots.Count < maxSlotCount)
            {
                int toAdd = Math.Min(remaining, item.maxStackCount);
                _slots.Add(new ItemStack(item, toAdd));
                remaining -= toAdd;
            }

            OnInventoryChanged?.Invoke();
            return remaining == 0;
        }

        // ──────────────────────────────────────
        //  아이템 제거
        // ──────────────────────────────────────
        /// <summary>
        /// itemID에 해당하는 아이템을 amount만큼 제거한다.
        /// 여러 슬롯에 걸쳐 있으면 뒤쪽 슬롯부터 차감한다.
        /// </summary>
        public bool RemoveItem(string itemID, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemID) || amount <= 0)
                return false;

            if (!HasItem(itemID, amount))
                return false;

            int remaining = amount;

            for (int i = _slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (_slots[i].data.itemID != itemID)
                    continue;

                int toRemove = Math.Min(remaining, _slots[i].currentStack);
                _slots[i].RemoveStack(toRemove);
                remaining -= toRemove;

                if (_slots[i].IsEmpty())
                    _slots.RemoveAt(i);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        // ──────────────────────────────────────
        //  슬롯 직접 제거 (장착 시 사용)
        // ──────────────────────────────────────
        /// <summary>특정 슬롯 인덱스의 아이템을 통째로 제거하고 반환한다.</summary>
        public ItemStack RemoveAt(int slotIndex)
        {
            if (!IsValidIndex(slotIndex))
                return null;

            var stack = _slots[slotIndex];
            _slots.RemoveAt(slotIndex);
            OnInventoryChanged?.Invoke();
            return stack;
        }

        // ──────────────────────────────────────
        //  아이템 사용 (소비 / 장비 분기)
        // ──────────────────────────────────────
        /// <summary>
        /// 슬롯의 아이템을 "사용"한다.
        /// - 장비(Weapon/Armor) → EquipmentManager.Equip 호출
        /// - 소비(Consumable) → 효과 적용 후 수량 차감
        /// </summary>
        public void UseItem(int slotIndex)
        {
            if (!IsValidIndex(slotIndex))
                return;

            var stack = _slots[slotIndex];

            switch (stack.data.itemType)
            {
                case ItemType.Weapon:
                case ItemType.Armor:
                    EquipFromSlot(slotIndex);
                    break;

                case ItemType.Consumable:
                    ConsumeFromSlot(slotIndex);
                    break;
            }
        }

        private void EquipFromSlot(int slotIndex)
        {
            var stack = RemoveAt(slotIndex);
            if (stack == null) return;

            // 기존 장비가 있으면 돌려받는다
            var previousEquip = EquipmentManager.Instance.Equip(stack);

            if (previousEquip != null)
                AddItem(previousEquip.data, previousEquip.currentStack);

            // RemoveAt에서 이미 이벤트 발행됨
        }

        private void ConsumeFromSlot(int slotIndex)
        {
            var stack = _slots[slotIndex];
            var consumable = stack.data as ConsumableData;
            if (consumable == null) return;

            // TODO: 실제 효과 적용 (힐, 버프 등)은 별도 시스템에서 처리
            // 예: PlayerHealth.Heal(consumable.healAmount);

            stack.RemoveStack(1);

            if (stack.IsEmpty())
                _slots.RemoveAt(slotIndex);

            OnInventoryChanged?.Invoke();
        }

        // ──────────────────────────────────────
        //  슬롯 스왑
        // ──────────────────────────────────────
        public void SwapSlots(int from, int to)
        {
            if (!IsValidIndex(from) || !IsValidIndex(to) || from == to)
                return;

            (_slots[from], _slots[to]) = (_slots[to], _slots[from]);
            OnInventoryChanged?.Invoke();
        }

        // ──────────────────────────────────────
        //  정렬
        // ──────────────────────────────────────
        public void SortInventory()
        {
            _slots.Sort((a, b) =>
            {
                int typeCompare = a.data.itemType.CompareTo(b.data.itemType);
                if (typeCompare != 0) return typeCompare;

                int rarityCompare = b.data.rarity.CompareTo(a.data.rarity); // 높은 등급 먼저
                if (rarityCompare != 0) return rarityCompare;

                return string.Compare(a.data.itemName, b.data.itemName, StringComparison.Ordinal);
            });

            OnInventoryChanged?.Invoke();
        }

        // ──────────────────────────────────────
        //  조회
        // ──────────────────────────────────────
        /// <summary>특정 아이템이 최소 amount개 이상 있는지 확인.</summary>
        public bool HasItem(string itemID, int amount = 1)
        {
            int total = 0;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].data.itemID == itemID)
                {
                    total += _slots[i].currentStack;
                    if (total >= amount) return true;
                }
            }
            return false;
        }

        /// <summary>특정 슬롯의 아이템을 반환 (읽기 전용 조회).</summary>
        public ItemStack GetSlot(int index)
        {
            return IsValidIndex(index) ? _slots[index] : null;
        }

        /// <summary>현재 사용 중인 슬롯 수.</summary>
        public int UsedSlotCount => _slots.Count;

        /// <summary>빈 슬롯이 남아 있는지.</summary>
        public bool HasEmptySlot => _slots.Count < maxSlotCount;

        // ──────────────────────────────────────
        //  내부 유틸
        // ──────────────────────────────────────
        private bool IsValidIndex(int index) => index >= 0 && index < _slots.Count;
    }
}
