using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableData : ItemData
{
    public int healAmount;
    public float buffDuration;
    public float cooldown;
}
