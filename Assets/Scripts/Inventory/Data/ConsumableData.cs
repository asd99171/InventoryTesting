using UnityEngine;

namespace Inventory.Data
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable Data")]
    public class ConsumableData : ItemData
    {
        [Header("소비 효과")]
        public int healAmount;
        public float buffDuration;
        public float cooldown;
    }
}
