using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Inventory.Logic;

namespace Inventory.UI
{
    /// <summary>
    /// 인벤토리 패널. 슬롯 프리팹을 풀링하여 생성/갱신한다.
    /// InventoryManager.OnInventoryChanged 구독.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private SlotUI slotPrefab;
        [SerializeField] private Button sortButton;
        [SerializeField] private Button closeButton;

        private readonly List<SlotUI> _slotPool = new List<SlotUI>();
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        // ──────────────────────────────────────
        //  라이프사이클
        // ──────────────────────────────────────
        private void Start()
        {
            // 최대 슬롯 수만큼 미리 풀 생성
            int maxSlots = InventoryManager.Instance.MaxSlotCount;
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = Instantiate(slotPrefab, slotContainer);
                slot.ClearSlot();
                _slotPool.Add(slot);
            }

            sortButton?.onClick.AddListener(OnSortClicked);
            closeButton?.onClick.AddListener(Close);

            panel.SetActive(false);
            _isOpen = false;
        }

        private void OnEnable()
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshSlots;
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged -= RefreshSlots;
        }

        // ──────────────────────────────────────
        //  열기 / 닫기
        // ──────────────────────────────────────
        public void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        public void Open()
        {
            panel.SetActive(true);
            _isOpen = true;
            RefreshSlots();
        }

        public void Close()
        {
            panel.SetActive(false);
            _isOpen = false;
            TooltipUI.Instance?.Hide();
        }

        // ──────────────────────────────────────
        //  슬롯 갱신
        // ──────────────────────────────────────
        private void RefreshSlots()
        {
            if (!_isOpen) return;

            var inv = InventoryManager.Instance;
            var slots = inv.Slots;

            for (int i = 0; i < _slotPool.Count; i++)
            {
                if (i < slots.Count && slots[i] != null)
                    _slotPool[i].SetSlot(slots[i], i, SlotUI.SlotContext.Inventory);
                else
                    _slotPool[i].ClearSlot();
            }
        }

        // ──────────────────────────────────────
        //  버튼 콜백
        // ──────────────────────────────────────
        private void OnSortClicked()
        {
            InventoryManager.Instance.SortInventory();
        }
    }
}
