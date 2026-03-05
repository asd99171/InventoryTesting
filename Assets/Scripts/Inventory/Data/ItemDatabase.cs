using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> items = new List<ItemData>();

        private Dictionary<string, ItemData> _lookup;

        public ItemData GetItemByID(string id)
        {
            if (_lookup == null)
                BuildLookup();

            _lookup.TryGetValue(id, out var item);
            return item;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, ItemData>();
            foreach (var item in items)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemID))
                    _lookup[item.itemID] = item;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
