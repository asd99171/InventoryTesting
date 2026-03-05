using System;

namespace InventorySystem.Data
{
    [Serializable]
    public class ItemStack
    {
        public ItemData data;
        public int currentStack;

        public ItemStack(ItemData data, int amount = 1)
        {
            this.data = data;
            this.currentStack = Math.Min(amount, data.maxStackCount);
        }

        /// <summary>
        /// 스택에 수량을 추가한다. 넘치는 잔여분을 반환한다.
        /// </summary>
        public int AddStack(int amount)
        {
            int space = data.maxStackCount - currentStack;
            if (amount <= space)
            {
                currentStack += amount;
                return 0;
            }

            currentStack = data.maxStackCount;
            return amount - space;
        }

        /// <summary>
        /// 스택에서 수량을 제거한다. 수량이 부족하면 false를 반환한다.
        /// </summary>
        public bool RemoveStack(int amount)
        {
            if (currentStack < amount)
                return false;

            currentStack -= amount;
            return true;
        }

        public bool IsStackFull()
        {
            return currentStack >= data.maxStackCount;
        }
    }
}
