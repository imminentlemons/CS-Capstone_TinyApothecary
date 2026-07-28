using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot_UI : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public Storage_UI storageUI;

    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public int slotID;
    public Inventory inventory;

    [SerializeField] private GameObject selector;

    public void SetSelector(bool isOn)
    {

        selector.SetActive(isOn);
    }

    public void Setitem(Inventory.Slot slot)
    {
        if (slot != null)
        {
            itemIcon.sprite = slot.icon;
            itemIcon.color = new Color(1, 1, 1, 1);
            quantityText.text = slot.count.ToString();
        }
    }

    public void SetEmpty()
    {
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0);
        quantityText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (storageUI != null)
        {
            storageUI.WithdrawSlotByClick(slotID);
        }
    }
}
