using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon Data")]
    public class WeaponData : ItemData
    {
        [Header("무기 스탯")]
        public int attackPower;
        public float attackSpeed;
        public float weaponRange;
    }
}
