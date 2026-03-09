using UnityEngine;

namespace Inventory.UI
{
    /// <summary>
    /// 키 입력으로 인벤토리/장비창 패널을 토글한다.
    /// Canvas 루트에 붙이면 된다.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("패널 참조")]
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private EquipmentUI equipmentUI;

        [Header("키 바인딩")]
        [SerializeField] private KeyCode inventoryKey = KeyCode.I;
        [SerializeField] private KeyCode equipmentKey = KeyCode.E;
        [SerializeField] private KeyCode closeAllKey = KeyCode.Escape;

        private void Update()
        {
            if (Input.GetKeyDown(inventoryKey))
                inventoryUI?.Toggle();

            if (Input.GetKeyDown(equipmentKey))
                equipmentUI?.Toggle();

            if (Input.GetKeyDown(closeAllKey))
                CloseAll();
        }

        public void CloseAll()
        {
            if (inventoryUI != null && inventoryUI.IsOpen)
                inventoryUI.Close();
            if (equipmentUI != null && equipmentUI.IsOpen)
                equipmentUI.Close();
            TooltipUI.Instance?.Hide();
        }
    }
}
