using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackText;
    [SerializeField] private GameObject emptyOverlay;

    private ItemStack currentStack;
    private int slotIndex;

    public void SetSlot(ItemStack stack, int index)
    {
        currentStack = stack;
        slotIndex = index;

        if (stack != null && stack.data != null)
        {
            iconImage.sprite = stack.data.icon;
            iconImage.enabled = true;

            if (stack.data.maxStackCount > 1 && stack.currentStack > 1)
            {
                stackText.text = stack.currentStack.ToString();
                stackText.enabled = true;
            }
            else
            {
                stackText.enabled = false;
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
        currentStack = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        stackText.enabled = false;

        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentStack == null) return;

        // 우클릭: 장착 또는 사용
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventoryManager.Instance.UseItem(slotIndex);
        }
    }
}
