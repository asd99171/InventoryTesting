using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon Data")]
    public class WeaponData : EquipmentData
    {
        [Header("무기 전용")]
        public int attackPower;
        public float attackSpeed;
        public float weaponRange;

        private void OnEnable()
        {
            itemType = ItemType.Weapon;
            maxStackCount = 1;
        }
    }
}
