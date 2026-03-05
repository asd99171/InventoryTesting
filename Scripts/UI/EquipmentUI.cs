using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    [Header("Equipment Slot References")]
    [SerializeField] private EquipmentSlotUI weaponSlot;
    [SerializeField] private EquipmentSlotUI headSlot;
    [SerializeField] private EquipmentSlotUI bodySlot;
    [SerializeField] private EquipmentSlotUI legsSlot;
    [SerializeField] private EquipmentSlotUI handsSlot;
    [SerializeField] private EquipmentSlotUI feetSlot;

    [Header("Stats Display")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI magicResistText;

    private void OnEnable()
    {
        EquipmentManager.Instance.OnEquipmentChanged += RefreshSlots;
        RefreshSlots();
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshSlots;
    }

    private void RefreshSlots()
    {
        var manager = EquipmentManager.Instance;

        weaponSlot.SetEquipment(manager.GetEquippedWeapon());
        headSlot.SetEquipment(manager.GetEquippedArmor(ArmorSlotType.Head));
        bodySlot.SetEquipment(manager.GetEquippedArmor(ArmorSlotType.Body));
        legsSlot.SetEquipment(manager.GetEquippedArmor(ArmorSlotType.Legs));
        handsSlot.SetEquipment(manager.GetEquippedArmor(ArmorSlotType.Hands));
        feetSlot.SetEquipment(manager.GetEquippedArmor(ArmorSlotType.Feet));

        RefreshStats(manager.GetTotalStats());
    }

    private void RefreshStats(EquipmentStats stats)
    {
        if (attackText != null) attackText.text = $"ATK: {stats.totalAttack}";
        if (defenseText != null) defenseText.text = $"DEF: {stats.totalDefense}";
        if (magicResistText != null) magicResistText.text = $"MR: {stats.totalMagicResist}";
    }
}

[Serializable]
public class EquipmentSlotUI
{
    public Image iconImage;
    public TextMeshProUGUI labelText;

    public void SetEquipment(ItemStack stack)
    {
        if (stack != null && stack.data != null)
        {
            iconImage.sprite = stack.data.icon;
            iconImage.enabled = true;
            if (labelText != null)
                labelText.text = stack.data.itemName;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            if (labelText != null)
                labelText.text = "";
        }
    }
}
