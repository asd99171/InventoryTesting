using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor")]
public class ArmorData : ItemData
{
    public ArmorSlotType slotType;
    public int defense;
    public int magicResistance;
}
