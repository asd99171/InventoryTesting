using UnityEngine;

namespace InventorySystem.Data
{
    public abstract class EquipmentData : ItemData
    {
        [Header("장비 공통")]
        public int requiredLevel = 1;
        public StatModifier[] statModifiers;
    }
}
