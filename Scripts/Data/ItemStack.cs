using System;

[Serializable]
public class ItemStack
{
    public ItemData data;
    public int currentStack;

    public ItemStack(ItemData data, int amount = 1)
    {
        this.data = data;
        this.currentStack = amount;
    }

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

    public bool RemoveStack(int amount)
    {
        if (currentStack < amount) return false;
        currentStack -= amount;
        return true;
    }

    public bool IsStackFull()
    {
        return currentStack >= data.maxStackCount;
    }
}
