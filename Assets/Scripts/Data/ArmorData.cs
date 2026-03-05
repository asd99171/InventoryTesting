using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "New Armor", menuName = "Inventory/Armor Data")]
    public class ArmorData : EquipmentData
    {
        [Header("방어구 전용")]
        public ArmorSlotType slotType;
        public int defense;
        public int magicResistance;

        private void OnEnable()
        {
            itemType = ItemType.Armor;
            maxStackCount = 1;
        }
    }
}
