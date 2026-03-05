using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable Data")]
    public class ConsumableData : ItemData
    {
        [Header("소비 아이템 스탯")]
        public int healAmount;
        public float buffDuration;
        public float cooldown;
    }
}
