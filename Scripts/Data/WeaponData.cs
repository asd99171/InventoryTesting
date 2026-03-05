using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    public int attackPower;
    public float attackSpeed;
    public float weaponRange;
}
