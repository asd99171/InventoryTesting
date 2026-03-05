using UnityEngine;

namespace Inventory.Data
{
    [CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor Data")]
    public class ArmorData : ItemData
    {
        [Header("방어구 스탯")]
        public ArmorSlotType slotType;
        public int defense;
        public int magicResistance;
    }
}
