using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Data;

namespace InventorySystem.Logic
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        // ── Events (UI 구독용) ──────────────────────────────
        /// <summary>인벤토리 내용이 변경될 때 발행</summary>
        public event Action OnInventoryChanged;

        /// <summary>아이템 추가 시 발행 (추가된 아이템 데이터, 수량)</summary>
        public event Action<ItemData, int> OnItemAdded;

        /// <summary>아이템 제거 시 발행 (제거된 아이템 데이터, 수량)</summary>
        public event Action<ItemData, int> OnItemRemoved;

        /// <summary>인벤토리가 가득 차서 아이템을 추가할 수 없을 때 발행</summary>
        public event Action OnInventoryFull;

        // ── Fields ──────────────────────────────────────────
        [SerializeField] private int maxSlotCount = 20;

        private List<ItemStack> slots;

        public IReadOnlyList<ItemStack> Slots => slots;
        public int MaxSlotCount => maxSlotCount;

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

            slots = new List<ItemStack>(new ItemStack[maxSlotCount]);
        }

        // ── Public API ──────────────────────────────────────

        /// <summary>
        /// 아이템을 인벤토리에 추가한다.
        /// 기존 스택에 병합을 시도하고, 남은 수량은 빈 슬롯에 배치한다.
        /// </summary>
        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            int remaining = amount;

            // 1) 기존 스택에 병합 시도
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (slots[i] != null && slots[i].data.itemID == item.itemID && !slots[i].IsStackFull())
                {
                    remaining = slots[i].AddStack(remaining);
                }
            }

            // 2) 빈 슬롯에 새 스택 생성
            while (remaining > 0)
            {
                int emptyIndex = FindEmptySlot();
                if (emptyIndex < 0)
                {
                    OnInventoryFull?.Invoke();
                    return false;
                }

                int stackAmount = Math.Min(remaining, item.maxStackCount);
                slots[emptyIndex] = new ItemStack(item, stackAmount);
                remaining -= stackAmount;
            }

            OnItemAdded?.Invoke(item, amount);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 지정 아이템ID의 수량을 제거한다.
        /// </summary>
        public bool RemoveItem(string itemID, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemID) || amount <= 0) return false;
            if (!HasItem(itemID, amount)) return false;

            int remaining = amount;
            ItemData removedData = null;

            for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (slots[i] == null || slots[i].data.itemID != itemID)
                    continue;

                removedData = slots[i].data;
                int removeCount = Math.Min(remaining, slots[i].currentStack);

                slots[i].RemoveStack(removeCount);
                remaining -= removeCount;

                if (slots[i].currentStack <= 0)
                    slots[i] = null;
            }

            if (removedData != null)
                OnItemRemoved?.Invoke(removedData, amount);

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 슬롯 인덱스로 아이템을 사용한다.
        /// 장비 → EquipmentManager.Equip, 소비 → 효과 적용 후 수량 차감
        /// </summary>
        public void UseItem(int slotIndex)
        {
            if (!IsValidSlot(slotIndex) || slots[slotIndex] == null) return;

            ItemStack stack = slots[slotIndex];

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

        /// <summary>
        /// 두 슬롯의 내용을 서로 교환한다.
        /// </summary>
        public void SwapSlots(int from, int to)
        {
            if (!IsValidSlot(from) || !IsValidSlot(to) || from == to) return;

            (slots[from], slots[to]) = (slots[to], slots[from]);
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// 인벤토리를 아이템 타입 → 이름 순으로 정렬한다.
        /// 빈 슬롯은 뒤로 보낸다.
        /// </summary>
        public void SortInventory()
        {
            slots.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;

                int typeCompare = a.data.itemType.CompareTo(b.data.itemType);
                if (typeCompare != 0) return typeCompare;

                return string.Compare(a.data.itemName, b.data.itemName, StringComparison.Ordinal);
            });

            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// 특정 아이템이 지정 수량 이상 존재하는지 확인한다.
        /// </summary>
        public bool HasItem(string itemID, int amount = 1)
        {
            int total = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].data.itemID == itemID)
                {
                    total += slots[i].currentStack;
                    if (total >= amount) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 아이템ID로 검색하여 첫 번째로 발견된 슬롯 인덱스를 반환한다.
        /// 없으면 -1을 반환한다.
        /// </summary>
        public int FindItem(string itemID)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].data.itemID == itemID)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 특정 슬롯의 아이템 스택을 반환한다.
        /// </summary>
        public ItemStack GetSlot(int index)
        {
            return IsValidSlot(index) ? slots[index] : null;
        }

        /// <summary>
        /// 특정 슬롯에 아이템 스택을 직접 설정한다.
        /// EquipmentManager에서 장비 해제 시 인벤토리에 아이템을 돌려줄 때 사용.
        /// </summary>
        public bool SetSlot(int index, ItemStack stack)
        {
            if (!IsValidSlot(index)) return false;

            slots[index] = stack;
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 특정 슬롯을 비운다.
        /// </summary>
        public void ClearSlot(int index)
        {
            if (!IsValidSlot(index)) return;

            slots[index] = null;
            OnInventoryChanged?.Invoke();
        }

        // ── Private Helpers ─────────────────────────────────

        private int FindEmptySlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                    return i;
            }
            return -1;
        }

        private bool IsValidSlot(int index)
        {
            return index >= 0 && index < slots.Count;
        }

        private void EquipFromSlot(int slotIndex)
        {
            if (EquipmentManager.Instance == null) return;

            ItemStack stack = slots[slotIndex];
            slots[slotIndex] = null;

            ItemStack previouslyEquipped = EquipmentManager.Instance.Equip(stack);

            if (previouslyEquipped != null)
                slots[slotIndex] = previouslyEquipped;

            OnInventoryChanged?.Invoke();
        }

        private void ConsumeFromSlot(int slotIndex)
        {
            ItemStack stack = slots[slotIndex];

            // 소비 효과는 PlayerStats나 외부 시스템에서 처리할 수 있도록
            // 여기서는 수량 차감만 수행
            stack.RemoveStack(1);

            if (stack.currentStack <= 0)
                slots[slotIndex] = null;

            OnInventoryChanged?.Invoke();
        }
    }
}
