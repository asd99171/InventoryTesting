using System;
using UnityEngine;
using Inventory.Data;

namespace Inventory.Logic
{
    /// <summary>
    /// 장착 장비 기반으로 합산 스탯을 계산하는 클래스.
    /// EquipmentManager.OnEquipmentChanged를 구독하여 자동 갱신된다.
    /// </summary>
    public class PlayerStats : SingletonMono<PlayerStats>
    {
        // ───────── 기본 스탯 (장비 미착용 시 기본값) ─────────
        [Header("기본 스탯")]
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private int baseMagicResist = 3;

        // ───────── 장비 합산 스탯 (캐시) ─────────
        private EquipmentBonus _equipmentBonus;

        // ───────── 최종 스탯 (기본 + 장비) ─────────
        public int TotalAttack => baseAttack + _equipmentBonus.attack;
        public int TotalDefense => baseDefense + _equipmentBonus.defense;
        public int TotalMagicResist => baseMagicResist + _equipmentBonus.magicResist;

        // ───────── 장비 보너스만 조회 ─────────
        public EquipmentBonus GetEquipmentBonus() => _equipmentBonus;

        // ───────── 이벤트 (UI 구독용) ─────────
        /// <summary>스탯이 재계산될 때마다 발행.</summary>
        public event Action OnStatsChanged;

        // ──────────────────────────────────────
        //  초기화 / 정리
        // ──────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            _equipmentBonus = new EquipmentBonus();
        }

        private void OnEnable()
        {
            // EquipmentManager가 아직 생성되지 않았을 수 있으므로 안전하게 접근
            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.OnEquipmentChanged += Recalculate;
        }

        private void OnDisable()
        {
            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.OnEquipmentChanged -= Recalculate;
        }

        // ──────────────────────────────────────
        //  스탯 재계산
        // ──────────────────────────────────────
        /// <summary>장착 장비를 전부 순회하며 보너스를 합산한다.</summary>
        public void Recalculate()
        {
            var equip = EquipmentManager.Instance;
            if (equip == null) return;

            int attack = 0;
            int defense = 0;
            int magicResist = 0;

            // 무기
            if (equip.EquippedWeapon != null && equip.EquippedWeapon.data is WeaponData weapon)
            {
                attack += weapon.attackPower;
            }

            // 방어구 5부위
            foreach (var kvp in equip.EquippedArmors)
            {
                if (kvp.Value != null && kvp.Value.data is ArmorData armor)
                {
                    defense += armor.defense;
                    magicResist += armor.magicResistance;
                }
            }

            _equipmentBonus = new EquipmentBonus(attack, defense, magicResist);
            OnStatsChanged?.Invoke();
        }
    }

    /// <summary>장비로부터 얻는 보너스 스탯을 담는 불변 값 타입.</summary>
    public readonly struct EquipmentBonus
    {
        public readonly int attack;
        public readonly int defense;
        public readonly int magicResist;

        public EquipmentBonus(int attack, int defense, int magicResist)
        {
            this.attack = attack;
            this.defense = defense;
            this.magicResist = magicResist;
        }

        public override string ToString()
        {
            return $"ATK +{attack} / DEF +{defense} / MRES +{magicResist}";
        }
    }
}
