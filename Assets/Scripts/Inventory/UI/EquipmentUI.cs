using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Data;
using Inventory.Logic;

namespace Inventory.UI
{
    /// <summary>
    /// 장비창 패널. 무기 1슬롯 + 방어구 5슬롯(고정) + 스탯 표시.
    /// EquipmentManager.OnEquipmentChanged, PlayerStats.OnStatsChanged 구독.
    /// </summary>
    public class EquipmentUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        [Header("장비 슬롯 (Inspector에서 직접 할당)")]
        [SerializeField] private SlotUI weaponSlot;
        [SerializeField] private SlotUI headSlot;
        [SerializeField] private SlotUI bodySlot;
        [SerializeField] private SlotUI legsSlot;
        [SerializeField] private SlotUI handsSlot;
        [SerializeField] private SlotUI feetSlot;

        [Header("스탯 텍스트")]
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private TextMeshProUGUI magicResistText;

        private bool _isOpen;
        public bool IsOpen => _isOpen;

        // ──────────────────────────────────────
        //  라이프사이클
        // ──────────────────────────────────────
        private void Start()
        {
            closeButton?.onClick.AddListener(Close);
            panel.SetActive(false);
            _isOpen = false;
        }

        private void OnEnable()
        {
            EquipmentManager.Instance.OnEquipmentChanged += RefreshSlots;
            PlayerStats.Instance.OnStatsChanged += RefreshStats;
        }

        private void OnDisable()
        {
            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.OnEquipmentChanged -= RefreshSlots;
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.OnStatsChanged -= RefreshStats;
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
            RefreshStats();
        }

        public void Close()
        {
            panel.SetActive(false);
            _isOpen = false;
            TooltipUI.Instance?.Hide();
        }

        // ──────────────────────────────────────
        //  장비 슬롯 갱신
        // ──────────────────────────────────────
        private void RefreshSlots()
        {
            if (!_isOpen) return;

            var equip = EquipmentManager.Instance;

            // 무기
            SetEquipSlot(weaponSlot, equip.EquippedWeapon, SlotUI.SlotContext.EquipmentWeapon);

            // 방어구 5부위
            SetArmorSlot(headSlot, ArmorSlotType.Head, equip);
            SetArmorSlot(bodySlot, ArmorSlotType.Body, equip);
            SetArmorSlot(legsSlot, ArmorSlotType.Legs, equip);
            SetArmorSlot(handsSlot, ArmorSlotType.Hands, equip);
            SetArmorSlot(feetSlot, ArmorSlotType.Feet, equip);
        }

        private void SetEquipSlot(SlotUI slotUI, ItemStack stack, SlotUI.SlotContext context)
        {
            if (slotUI == null) return;

            if (stack != null)
                slotUI.SetSlot(stack, -1, context);
            else
                slotUI.ClearSlot();
        }

        private void SetArmorSlot(SlotUI slotUI, ArmorSlotType armorSlot, EquipmentManager equip)
        {
            if (slotUI == null) return;

            var stack = equip.GetEquippedArmor(armorSlot);
            if (stack != null)
                slotUI.SetSlot(stack, -1, SlotUI.SlotContext.EquipmentArmor, armorSlot);
            else
                slotUI.ClearSlot();
        }

        // ──────────────────────────────────────
        //  스탯 텍스트 갱신
        // ──────────────────────────────────────
        private void RefreshStats()
        {
            if (!_isOpen) return;

            var stats = PlayerStats.Instance;
            if (stats == null) return;

            if (attackText != null)
                attackText.text = $"ATK  {stats.TotalAttack}";
            if (defenseText != null)
                defenseText.text = $"DEF  {stats.TotalDefense}";
            if (magicResistText != null)
                magicResistText.text = $"MRES  {stats.TotalMagicResist}";
        }
    }
}
