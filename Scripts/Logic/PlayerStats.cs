using System;
using UnityEngine;
using InventorySystem.Data;

namespace InventorySystem.Logic
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        // ── Events (UI 구독용) ──────────────────────────────
        /// <summary>최종 스탯이 갱신될 때 발행</summary>
        public event Action<FinalStats> OnStatsChanged;

        // ── Base Stats ──────────────────────────────────────
        [Header("기본 스탯 (레벨/직업 등으로 결정)")]
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private int baseMagicResist = 3;
        [SerializeField] private float baseAttackSpeed = 1.0f;
        [SerializeField] private int baseMaxHP = 100;

        // ── Cached ──────────────────────────────────────────
        private FinalStats cachedStats;
        public FinalStats CurrentStats => cachedStats;

        // ── Unity Lifecycle ─────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += Recalculate;
            }

            Recalculate();
        }

        private void OnDestroy()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= Recalculate;
            }
        }

        // ── Public API ──────────────────────────────────────

        /// <summary>
        /// 기본 스탯 + 장비 스탯을 합산하여 최종 스탯을 재계산한다.
        /// </summary>
        public void Recalculate()
        {
            EquipmentStats equipStats = default;

            if (EquipmentManager.Instance != null)
                equipStats = EquipmentManager.Instance.GetTotalStats();

            cachedStats = new FinalStats
            {
                attack = baseAttack + equipStats.totalAttack,
                defense = baseDefense + equipStats.totalDefense,
                magicResist = baseMagicResist + equipStats.totalMagicResist,
                attackSpeed = baseAttackSpeed + equipStats.attackSpeed,
                maxHP = baseMaxHP
            };

            OnStatsChanged?.Invoke(cachedStats);
        }

        /// <summary>
        /// 기본 스탯을 변경한다 (레벨업 등).
        /// </summary>
        public void SetBaseStats(int attack, int defense, int magicResist, float attackSpeed, int maxHP)
        {
            baseAttack = attack;
            baseDefense = defense;
            baseMagicResist = magicResist;
            baseAttackSpeed = attackSpeed;
            baseMaxHP = maxHP;

            Recalculate();
        }
    }

    /// <summary>
    /// 기본 스탯 + 장비 스탯이 합산된 최종 플레이어 스탯
    /// </summary>
    public struct FinalStats
    {
        public int attack;
        public int defense;
        public int magicResist;
        public float attackSpeed;
        public int maxHP;

        public override string ToString()
        {
            return $"ATK:{attack} DEF:{defense} MR:{magicResist} SPD:{attackSpeed:F1} HP:{maxHP}";
        }
    }
}
