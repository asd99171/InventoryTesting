using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Inventory.Data;
using Inventory.Logic;

namespace Inventory.UI
{
    /// <summary>
    /// 인벤토리/장비창의 개별 슬롯 한 칸.
    /// 클릭(사용), 우클릭(해제), 드래그앤드롭을 지원한다.
    /// </summary>
    public class SlotUI : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [Header("UI 참조")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject emptyOverlay;

        // ───────── 슬롯 데이터 ─────────
        private ItemStack _itemStack;
        private int _slotIndex = -1;
        private SlotContext _context;

        // ───────── 드래그 상태 (static 공유) ─────────
        private static SlotUI _dragSource;
        private static GameObject _dragIcon;
        private static Canvas _rootCanvas;

        // ───────── 등급별 색상 ─────────
        private static readonly Color[] RarityColors =
        {
            new Color(0.75f, 0.75f, 0.75f), // Common
            new Color(0.30f, 0.78f, 0.30f), // Uncommon
            new Color(0.25f, 0.50f, 0.95f), // Rare
            new Color(0.60f, 0.30f, 0.90f), // Epic
            new Color(1.00f, 0.65f, 0.00f), // Legendary
        };

        public enum SlotContext
        {
            Inventory,
            EquipmentWeapon,
            EquipmentArmor
        }

        public ItemStack ItemStack => _itemStack;
        public int SlotIndex => _slotIndex;
        public SlotContext Context => _context;

        // 장비 슬롯인 경우 어떤 방어구 부위인지
        private ArmorSlotType _armorSlotType;
        public ArmorSlotType ArmorSlotType => _armorSlotType;

        // ──────────────────────────────────────
        //  슬롯 갱신
        // ──────────────────────────────────────
        public void SetSlot(ItemStack stack, int index, SlotContext context, ArmorSlotType armorSlot = default)
        {
            _itemStack = stack;
            _slotIndex = index;
            _context = context;
            _armorSlotType = armorSlot;

            if (stack != null && stack.data != null)
            {
                iconImage.sprite = stack.data.icon;
                iconImage.color = Color.white;
                iconImage.enabled = true;

                stackText.text = stack.currentStack > 1 ? stack.currentStack.ToString() : "";
                stackText.enabled = true;

                if (rarityBorder != null)
                {
                    int idx = (int)stack.data.rarity;
                    rarityBorder.color = idx < RarityColors.Length ? RarityColors[idx] : Color.white;
                    rarityBorder.enabled = true;
                }

                if (emptyOverlay != null)
                    emptyOverlay.SetActive(false);
            }
            else
            {
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            _itemStack = null;

            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconImage.enabled = false;

            stackText.text = "";
            stackText.enabled = false;

            if (rarityBorder != null)
            {
                rarityBorder.color = new Color(0.3f, 0.3f, 0.3f);
                rarityBorder.enabled = true;
            }

            if (emptyOverlay != null)
                emptyOverlay.SetActive(true);
        }

        // ──────────────────────────────────────
        //  클릭
        // ──────────────────────────────────────
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_itemStack == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 인벤토리 슬롯 → 아이템 사용 (장착/소비)
                if (_context == SlotContext.Inventory)
                    InventoryManager.Instance.UseItem(_slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 장비 슬롯 → 해제하여 인벤토리로
                if (_context == SlotContext.EquipmentWeapon)
                    EquipmentManager.Instance.UnequipWeaponToInventory();
                else if (_context == SlotContext.EquipmentArmor)
                    EquipmentManager.Instance.UnequipToInventory(_armorSlotType);
            }
        }

        // ──────────────────────────────────────
        //  툴팁
        // ──────────────────────────────────────
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_itemStack != null)
                TooltipUI.Instance?.Show(_itemStack.data, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.Instance?.Hide();
        }

        // ──────────────────────────────────────
        //  드래그앤드롭
        // ──────────────────────────────────────
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_itemStack == null || _context != SlotContext.Inventory)
                return;

            _dragSource = this;

            // 루트 캔버스 캐시
            if (_rootCanvas == null)
                _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

            // 드래그 아이콘 생성
            _dragIcon = new GameObject("DragIcon");
            _dragIcon.transform.SetParent(_rootCanvas.transform, false);

            var img = _dragIcon.AddComponent<Image>();
            img.sprite = _itemStack.data.icon;
            img.raycastTarget = false;
            img.SetNativeSize();

            // 크기 조정 (슬롯과 비슷하게)
            var rt = _dragIcon.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 60);

            // 반투명
            var c = img.color;
            c.a = 0.8f;
            img.color = c;

            // 원본 아이콘 반투명 처리
            iconImage.color = new Color(1f, 1f, 1f, 0.3f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragIcon == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint
            );

            _dragIcon.transform.localPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 아이콘 복원
            if (_itemStack != null)
                iconImage.color = Color.white;

            CleanupDrag();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_dragSource == null || _dragSource == this)
                return;

            // 인벤토리 슬롯 간 스왑
            if (_dragSource.Context == SlotContext.Inventory && _context == SlotContext.Inventory)
            {
                InventoryManager.Instance.SwapSlots(_dragSource.SlotIndex, _slotIndex);
            }
            // 인벤토리 → 장비 슬롯으로 드래그 (장착)
            else if (_dragSource.Context == SlotContext.Inventory &&
                     (_context == SlotContext.EquipmentWeapon || _context == SlotContext.EquipmentArmor))
            {
                var dragItem = _dragSource.ItemStack;
                if (dragItem != null)
                {
                    bool isWeapon = dragItem.data.itemType == ItemType.Weapon && _context == SlotContext.EquipmentWeapon;
                    bool isArmor = dragItem.data.itemType == ItemType.Armor && _context == SlotContext.EquipmentArmor;

                    if (isWeapon || isArmor)
                        InventoryManager.Instance.UseItem(_dragSource.SlotIndex);
                }
            }

            CleanupDrag();
        }

        private static void CleanupDrag()
        {
            if (_dragIcon != null)
                Destroy(_dragIcon);

            _dragIcon = null;
            _dragSource = null;
        }
    }
}
