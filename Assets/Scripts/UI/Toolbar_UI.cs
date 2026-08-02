using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Toolbar_UI : MonoBehaviour

{
    public Player player;

    public InputType inputType;

    [SerializeField] private List<Slot_UI> toolbarSlots = new List<Slot_UI>();

    private Slot_UI selectedSlot;
    private Canvas canvas;
    private Slot_UI draggedSlot;
    private Image draggedIcon;
    private bool dragSingle;
    private GameObject inventoryPanel;

    public Inventory_UI inventoryUI;
    public InventoryManager ownerInventory;
    private Inventory toolbar;

    public IReadOnlyList<Slot_UI> ToolbarSlots => toolbarSlots;

    public int CurrentSlotIndex => currentSlot;

    public void SetGameplaySelectorVisible(bool visible)
    {
        if(selectedSlot != null)
        {
            selectedSlot.SetSelector(visible);
        }
    }

    public enum InputType
    {
        Keyboard,
        Gamepad
    }

    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
        inventoryPanel = inventoryUI.inventoryPanel;
    }
    private void Start()
    {

        toolbar = ownerInventory.GetInventoryByName("Toolbar");

        SetupToolbarSlots();
        Refresh();
        SelectSlot(0);
    }

    private void Update()
    {
        if (GameFlow_UI.GameplayUIInputBlocked)
        {
            return;
        }

        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            return;
        }

        if(inputType == InputType.Keyboard)
        {
            CheckAlphaNumbericKeys();
        }
        else
        {
            CheckGamepadControls();
        }

    }
   
    public void SelectSlot(int index)
    {
        if(toolbarSlots.Count == 7)
        {
            if(selectedSlot != null)
            {
                selectedSlot.SetSelector(false);
            }

            currentSlot = index;
            selectedSlot = toolbarSlots[currentSlot];
            selectedSlot.SetSelector(true);

            ownerInventory.toolbar.SelectSlot(index);

        }
    }  

    private void CheckAlphaNumbericKeys()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if(Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectSlot(2);
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            SelectSlot(3);
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            SelectSlot(4);
        }

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            SelectSlot(5);
        }

        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            SelectSlot(6);
        }

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll > 0f)
            {
                PreviousSlot();
            }
            else if (scroll < 0f)
            {
                NextSlot();
            }
        }
    }

    private void CheckGamepadControls()
    {
        if (Gamepad.current == null)
        {
            return;
        }
            

        if(Gamepad.current.leftTrigger.wasPressedThisFrame)
        {
            PreviousSlot();
        }
        if(Gamepad.current.rightTrigger.wasPressedThisFrame)
        {
            NextSlot();
        }
    }

    private int currentSlot = 0;

    private void NextSlot()
    {
        currentSlot = (currentSlot + 1) % toolbarSlots.Count;
        SelectSlot(currentSlot);
    }

    private void PreviousSlot()
    {
        currentSlot--;

        if(currentSlot < 0)
        {
            currentSlot = toolbarSlots.Count - 1;
        }

        SelectSlot(currentSlot);

    }

    public void SlotBeginDrag(Slot_UI slot)
    {
        draggedSlot = slot;

        dragSingle = Keyboard.current != null &&
                 Keyboard.current.leftShiftKey.isPressed;

        draggedIcon = Instantiate(slot.itemIcon);
        draggedIcon.raycastTarget = false;
        draggedIcon.rectTransform.sizeDelta = new Vector2(50, 50);
        draggedIcon.transform.SetParent(canvas.transform);

        MoveToMousePosition(draggedIcon.gameObject);

        
    }

    public void SlotDrag()
    {
        if (draggedIcon != null)
        {
            MoveToMousePosition(draggedIcon.gameObject);
        }
    }

    public void SlotEndDrag()
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon.gameObject);
            draggedIcon = null;
        }
    }

    public void SlotDrop(Slot_UI slot)
    {        
        Slot_UI sourceSlot = inventoryUI.GetDraggedSlot();

        if (sourceSlot == null)
        {
            sourceSlot = draggedSlot;
        }
        if (sourceSlot == null)
        {
            return;
        }

        sourceSlot.inventory.MoveOrSwapStack(
            sourceSlot.slotID,
            slot.slotID,
            slot.inventory
        );

        inventoryUI.Refresh();
        Refresh();
        
        inventoryUI.ClearDraggedSlot();
        ClearDraggedSlot();
    }

    public void ClearDraggedSlot()
    {
        draggedSlot = null;
    }

    private void MoveToMousePosition(GameObject toMove)
    {
        if (canvas != null && Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePosition,
                null,
                out position);

            toMove.transform.position = canvas.transform.TransformPoint(position);
        }
    }

    public Slot_UI GetDraggedSlot() => draggedSlot;
    public bool IsDragSingle() => dragSingle;

    public void Refresh()
    {
        if(toolbarSlots.Count == toolbar.slots.Count)
        {
            for(int i = 0; i < toolbarSlots.Count; i++)
            {
                if(toolbar.slots[i].itemName != "")
                {
                    toolbarSlots[i].Setitem(toolbar.slots[i]);
                }
                else
                {
                    toolbarSlots[i].SetEmpty();
                }
            }
        }
    }

    public void MoveSelectedToBackpack()
    {
        if(selectedSlot == null)
        {
            return;
        }

        Inventory.Slot toolbarSlot = toolbar.slots[selectedSlot.slotID];

        if(toolbarSlot.IsEmpty)
        {
            return;
        }

        Item item = GameManager.instance.itemManager.GetItemByName(toolbarSlot.itemName);

        if(item == null)
        {
            return;
        }

        //check for room first so nothing is lost if backpack is full
        bool backpackHasRoom = false;

        foreach(Inventory.Slot backpackSlot in ownerInventory.backpack.slots)
        {
            if(backpackSlot.IsEmpty ||
                backpackSlot.CanAdditem(item.data.itemName))
            {
                backpackHasRoom = true;
                break;
            }
        }

        if(!backpackHasRoom)
        {
            NotificationPopup_UI.Show(player, "Backpack is full.");
            return;
        }

        ownerInventory.backpack.Add(item);
        toolbar.Remove(selectedSlot.slotID);

        Refresh();
        inventoryUI.Refresh();
    }

    public void SetupToolbarSlots()
    {
        int counter = 0;

        foreach(Slot_UI slot in toolbarSlots)
        {
            slot.slotID = counter;
            slot.inventory = toolbar;
            counter++;
        }
    }


}

