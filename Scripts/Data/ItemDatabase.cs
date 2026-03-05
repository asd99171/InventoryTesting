using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> items = new List<ItemData>();

        public ItemData GetItemByID(string id)
        {
            return items.Find(item => item.itemID == id);
        }
    }
}
