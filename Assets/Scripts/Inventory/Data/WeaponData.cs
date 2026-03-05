using UnityEngine;

namespace Inventory.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon Data")]
    public class WeaponData : ItemData
    {
        [Header("무기 스탯")]
        public int attackPower;
        public float attackSpeed = 1f;
        public float weaponRange = 1f;
    }
}
