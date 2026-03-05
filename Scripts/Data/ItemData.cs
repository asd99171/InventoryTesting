using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;
    public Rarity rarity;
    public int maxStackCount = 1;
    public int sellPrice;
}
