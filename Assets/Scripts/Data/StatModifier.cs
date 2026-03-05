using System;

namespace InventorySystem.Data
{
    [Serializable]
    public struct StatModifier
    {
        public StatType statType;
        public float value;

        public StatModifier(StatType statType, float value)
        {
            this.statType = statType;
            this.value = value;
        }
    }
}
