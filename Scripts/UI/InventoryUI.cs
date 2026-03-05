using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;

    private readonly List<SlotUI> slotUIList = new List<SlotUI>();

    private void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += RefreshSlots;
        RefreshSlots();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshSlots;
    }

    private void RefreshSlots()
    {
        var slots = InventoryManager.Instance.Slots;
        int maxSlots = InventoryManager.Instance.MaxSlotCount;

        // 슬롯 UI 수 맞추기
        while (slotUIList.Count < maxSlots)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            SlotUI slotUI = go.GetComponent<SlotUI>();
            slotUIList.Add(slotUI);
        }

        // 슬롯 데이터 반영
        for (int i = 0; i < maxSlots; i++)
        {
            if (i < slots.Count)
                slotUIList[i].SetSlot(slots[i], i);
            else
                slotUIList[i].ClearSlot();
        }
    }
}
