using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable Data")]
    public class ConsumableData : ItemData
    {
        [Header("소비 아이템 전용")]
        public int healAmount;
        public float buffDuration;
        public float cooldown;

        private void OnEnable()
        {
            itemType = ItemType.Consumable;
        }
    }
}
