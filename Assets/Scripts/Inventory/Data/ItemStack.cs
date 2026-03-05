using System;

namespace Inventory.Data
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

        /// <summary>스택 추가. 넘치는 잔여분을 반환 (0이면 전부 수용됨).</summary>
        public int AddStack(int amount)
        {
            int space = data.maxStackCount - currentStack;
            int toAdd = Math.Min(amount, space);
            currentStack += toAdd;
            return amount - toAdd;
        }

        /// <summary>수량 차감. 수량 부족 시 false 반환.</summary>
        public bool RemoveStack(int amount)
        {
            if (currentStack < amount)
                return false;

            currentStack -= amount;
            return true;
        }

        public bool IsStackFull() => currentStack >= data.maxStackCount;

        public bool IsEmpty() => currentStack <= 0;
    }
}
