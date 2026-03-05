using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    [SerializeField] private int maxSlotCount = 20;
    private List<ItemStack> slots;

    public IReadOnlyList<ItemStack> Slots => slots;
    public int MaxSlotCount => maxSlotCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        slots = new List<ItemStack>(maxSlotCount);
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        // 기존 스택에 추가 시도
        foreach (var slot in slots)
        {
            if (slot.data.itemID == item.itemID && !slot.IsStackFull())
            {
                amount = slot.AddStack(amount);
                if (amount == 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // 새 슬롯에 할당
        while (amount > 0 && slots.Count < maxSlotCount)
        {
            int stackAmount = Mathf.Min(amount, item.maxStackCount);
            slots.Add(new ItemStack(item, stackAmount));
            amount -= stackAmount;
        }

        OnInventoryChanged?.Invoke();
        return amount == 0;
    }

    public bool RemoveItem(string itemID, int amount = 1)
    {
        int remaining = amount;

        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (slots[i].data.itemID != itemID) continue;

            if (slots[i].currentStack <= remaining)
            {
                remaining -= slots[i].currentStack;
                slots.RemoveAt(i);
            }
            else
            {
                slots[i].RemoveStack(remaining);
                remaining = 0;
            }
        }

        if (remaining < amount)
            OnInventoryChanged?.Invoke();

        return remaining == 0;
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        ItemStack stack = slots[slotIndex];

        switch (stack.data.itemType)
        {
            case ItemType.Weapon:
            case ItemType.Armor:
                ItemStack returned = EquipmentManager.Instance.Equip(stack);
                slots.RemoveAt(slotIndex);
                if (returned != null)
                    AddItem(returned.data, returned.currentStack);
                else
                    OnInventoryChanged?.Invoke();
                break;

            case ItemType.Consumable:
                stack.RemoveStack(1);
                if (stack.currentStack <= 0)
                    slots.RemoveAt(slotIndex);
                OnInventoryChanged?.Invoke();
                break;
        }
    }

    public void SwapSlots(int from, int to)
    {
        if (from < 0 || from >= slots.Count || to < 0 || to >= slots.Count) return;

        (slots[from], slots[to]) = (slots[to], slots[from]);
        OnInventoryChanged?.Invoke();
    }

    public void SortInventory()
    {
        slots.Sort((a, b) =>
        {
            int typeCompare = a.data.itemType.CompareTo(b.data.itemType);
            if (typeCompare != 0) return typeCompare;
            return string.Compare(a.data.itemName, b.data.itemName, StringComparison.Ordinal);
        });
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(string itemID, int amount = 1)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.data.itemID == itemID)
            {
                total += slot.currentStack;
                if (total >= amount) return true;
            }
        }
        return false;
    }
}
