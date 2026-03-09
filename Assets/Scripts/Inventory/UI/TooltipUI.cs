using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Data;

namespace Inventory.UI
{
    /// <summary>
    /// 아이템 호버 시 나타나는 툴팁 팝업.
    /// 싱글톤으로 씬 전체에서 하나만 존재한다.
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance { get; private set; }

        [Header("UI 참조")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Image iconImage;

        [Header("설정")]
        [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

        private RectTransform _tooltipRect;
        private RectTransform _canvasRect;

        private void Awake()
        {
            Instance = this;
            _tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            _canvasRect = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
            Hide();
        }

        public void Show(ItemData data, Vector3 slotWorldPos)
        {
            if (data == null) return;

            tooltipPanel.SetActive(true);

            // 기본 정보
            nameText.text = data.itemName;
            nameText.color = GetRarityColor(data.rarity);
            typeText.text = GetTypeLabel(data);
            descriptionText.text = data.description;

            // 스탯
            statsText.text = GetStatsText(data);
            statsText.gameObject.SetActive(!string.IsNullOrEmpty(statsText.text));

            // 아이콘
            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = data.icon != null;
            }

            // 위치 조정 (화면 밖으로 나가지 않도록)
            PositionTooltip(slotWorldPos);
        }

        public void Hide()
        {
            tooltipPanel.SetActive(false);
        }

        private void PositionTooltip(Vector3 worldPos)
        {
            // 월드 → 스크린 → 캔버스 로컬
            var screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos, null, out var localPos
            );

            localPos += offset;

            // 화면 밖 클램프
            var tooltipSize = _tooltipRect.sizeDelta;
            var canvasSize = _canvasRect.sizeDelta;
            var halfCanvas = canvasSize * 0.5f;

            float maxX = halfCanvas.x - tooltipSize.x;
            float minY = -halfCanvas.y + tooltipSize.y;

            if (localPos.x + tooltipSize.x > halfCanvas.x)
                localPos.x = maxX;
            if (localPos.y - tooltipSize.y < -halfCanvas.y)
                localPos.y = minY;

            _tooltipRect.localPosition = localPos;
        }

        private string GetStatsText(ItemData data)
        {
            switch (data)
            {
                case WeaponData w:
                    return $"공격력  +{w.attackPower}\n" +
                           $"공격속도  {w.attackSpeed:F1}\n" +
                           $"사거리  {w.weaponRange:F1}";

                case ArmorData a:
                    return $"방어력  +{a.defense}\n" +
                           $"마법저항  +{a.magicResistance}\n" +
                           $"부위  {GetSlotLabel(a.slotType)}";

                case ConsumableData c:
                    var lines = "";
                    if (c.healAmount > 0) lines += $"회복량  +{c.healAmount}\n";
                    if (c.buffDuration > 0) lines += $"버프 지속  {c.buffDuration:F1}초\n";
                    if (c.cooldown > 0) lines += $"쿨다운  {c.cooldown:F1}초";
                    return lines.TrimEnd('\n');

                default:
                    return "";
            }
        }

        private string GetTypeLabel(ItemData data)
        {
            string rarity = data.rarity.ToString();
            switch (data.itemType)
            {
                case ItemType.Weapon: return $"{rarity} 무기";
                case ItemType.Armor: return $"{rarity} 방어구";
                case ItemType.Consumable: return $"{rarity} 소비 아이템";
                default: return rarity;
            }
        }

        private string GetSlotLabel(ArmorSlotType slot)
        {
            switch (slot)
            {
                case ArmorSlotType.Head: return "머리";
                case ArmorSlotType.Body: return "몸통";
                case ArmorSlotType.Legs: return "다리";
                case ArmorSlotType.Hands: return "손";
                case ArmorSlotType.Feet: return "발";
                default: return slot.ToString();
            }
        }

        private Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return Color.white;
                case Rarity.Uncommon: return new Color(0.30f, 0.78f, 0.30f);
                case Rarity.Rare: return new Color(0.25f, 0.50f, 0.95f);
                case Rarity.Epic: return new Color(0.60f, 0.30f, 0.90f);
                case Rarity.Legendary: return new Color(1.00f, 0.65f, 0.00f);
                default: return Color.white;
            }
        }
    }
}
