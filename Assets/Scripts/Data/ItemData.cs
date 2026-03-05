using UnityEngine;

namespace InventorySystem.Data
{
    public abstract class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemID;
        public string itemName;
        public Sprite icon;
        [TextArea] public string description;

        [Header("분류")]
        public ItemType itemType;
        public Rarity rarity;

        [Header("스택 / 가격")]
        public int maxStackCount = 1;
        public int sellPrice;
    }
}
