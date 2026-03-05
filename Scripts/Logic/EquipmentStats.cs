namespace InventorySystem.Logic
{
    /// <summary>
    /// 장착 장비의 합산 스탯을 담는 구조체
    /// </summary>
    public struct EquipmentStats
    {
        public int totalAttack;
        public int totalDefense;
        public int totalMagicResist;
        public float attackSpeed;

        public override string ToString()
        {
            return $"ATK:{totalAttack} DEF:{totalDefense} MR:{totalMagicResist} SPD:{attackSpeed:F1}";
        }
    }
}
