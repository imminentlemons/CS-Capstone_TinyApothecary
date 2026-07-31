using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory_UI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public string inventoryName;

    public InventoryManager ownerInventory;
    public Player ownerPlayer;
    public Inventory toolbar;
    public Toolbar_UI toolbarUI;

    public List<Slot_UI> slots = new();
    public Toolbar_UI.InputType inputType;


    private Slot_UI selectedSlot;
    private int currentSlot = 0;
    private const int columns = 5;
    private float nextMoveTime;
    private Canvas canvas;
    private Slot_UI draggedSlot;
    private Image draggedIcon;
    private bool dragSingle;

    private Inventory inventory;

    private enum InventoryArea
    {
        Toolbar,
        Backpack
    }

    private InventoryArea currentArea;
    private int currentToolbarSlot;


    [SerializeField] private float moveDelay = 0.2f;

    void Start()
    {

        inventory = ownerInventory.GetInventoryByName(inventoryName);
        toolbar = ownerInventory.GetInventoryByName("Toolbar");
        SetupSlots();
        if (inventoryPanel == null)
        {
            return;
        }
        inventoryPanel.SetActive(false);
        Refresh();
    }

    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }
        if (inputType == Toolbar_UI.InputType.Gamepad)
        {
            CheckGamepadControls();
        }
        else
        {
            CheckKeyboardControls();
        }
    }


    public void ToggleInventory()
    {
        bool opening = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(opening);

        if(opening)
        {
            inventoryPanel.transform.SetAsLastSibling();

            if(inputType == Toolbar_UI.InputType.Gamepad)
            {
                currentArea = InventoryArea.Toolbar;
                currentToolbarSlot = toolbarUI.CurrentSlotIndex;
            }
            else
            {
                currentArea = InventoryArea.Backpack;
                currentSlot = 0;
            }

            RefreshSelector();
        }
        else
        {
            ClearSelectors();

            //restore regular gameplay toolbar selector
            toolbarUI.SetGameplaySelectorVisible(true);
        }

        Refresh();
    }

    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;
    

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
            return;

        if (selectedSlot != null)
            selectedSlot.SetSelector(false);

        selectedSlot = slots[index];
        selectedSlot.SetSelector(true);
        currentSlot = index;
    }

    private void MoveRight()
    {
        if (currentSlot % columns < columns - 1
            && currentSlot + 1 < slots.Count)
        {
            SelectSlot(currentSlot + 1);
        }
    }
    private void MoveLeft()
    {
        if (currentSlot % columns > 0)
        {
            SelectSlot(currentSlot - 1);
        }
    }
    private void MoveUp()
    {
        if (currentSlot - columns >= 0)
        {
            SelectSlot(currentSlot - columns);
        }
    }
    private void MoveDown()
    {
        if (currentSlot + columns < slots.Count)
        {
            SelectSlot(currentSlot + columns);
        }
    }

    private void MoveSelectionRight()
    {
        if (currentArea == InventoryArea.Toolbar)
        {
            if (currentToolbarSlot + 1 <
                toolbarUI.ToolbarSlots.Count)
            {
                currentToolbarSlot++;
            }
        }
        else
        {
            if (currentSlot % columns < columns - 1 &&
                currentSlot + 1 < slots.Count)
            {
                currentSlot++;
            }
        }

        RefreshSelector();
    }

    private void MoveSelectionLeft()
    {
        if (currentArea == InventoryArea.Toolbar)
        {
            if (currentToolbarSlot > 0)
            {
                currentToolbarSlot--;
            }
        }
        else
        {
            if (currentSlot % columns > 0)
            {
                currentSlot--;
            }
        }

        RefreshSelector();
    }

    private void MoveSelectionUp()
    {
        if (currentArea == InventoryArea.Toolbar)
        {
            currentArea = InventoryArea.Backpack;

            int backpackColumn =
                ToolbarToBackpackColumn(
                    currentToolbarSlot
                );

            int bottomRowStart =
                ((slots.Count - 1) / columns) * columns;

            currentSlot = Mathf.Min(
                bottomRowStart + backpackColumn,
                slots.Count - 1
            );
        }
        else if (currentSlot - columns >= 0)
        {
            currentSlot -= columns;
        }

        RefreshSelector();
    }

    private void MoveSelectionDown()
    {
        if (currentArea == InventoryArea.Toolbar)
        {
            return;
        }

        if (currentSlot + columns < slots.Count)
        {
            currentSlot += columns;
        }
        else
        {
            currentArea = InventoryArea.Toolbar;

            currentToolbarSlot =
                BackpackToToolbarIndex(currentSlot);
        }

        RefreshSelector();
    }

    private void CheckGamepadControls()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
        {
            return;
        }

        if (Time.time >= nextMoveTime)
        {
            Vector2 navigation =
                gamepad.leftStick.ReadValue();

            Vector2 dpad =
                gamepad.dpad.ReadValue();

            if (dpad.sqrMagnitude >
                navigation.sqrMagnitude)
            {
                navigation = dpad;
            }

            bool moved = false;

            if (navigation.x > 0.5f)
            {
                MoveSelectionRight();
                moved = true;
            }
            else if (navigation.x < -0.5f)
            {
                MoveSelectionLeft();
                moved = true;
            }
            else if (navigation.y > 0.5f)
            {
                MoveSelectionUp();
                moved = true;
            }
            else if (navigation.y < -0.5f)
            {
                MoveSelectionDown();
                moved = true;
            }

            if (moved)
            {
                nextMoveTime =
                    Time.time + moveDelay;
            }
        }

        // A transfers one item.
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            TransferSelectedItem();
        }

        // X is the contextual secondary action: drop the selected stack.
        if (gamepad.buttonWest.wasPressedThisFrame &&
            currentArea == InventoryArea.Backpack)
        {
            Remove();
        }

        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            CloseInventory();
        }
    }

    private void CheckKeyboardControls()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            MoveSelectionUp();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            MoveSelectionDown();
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            MoveSelectionLeft();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            MoveSelectionRight();
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TransferSelectedItem();
        }

        if (Keyboard.current.deleteKey.wasPressedThisFrame &&
            currentArea == InventoryArea.Backpack)
        {
            Remove();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseInventory();
        }
    }

    private void CloseInventory()
    {
        if (IsOpen)
        {
            ToggleInventory();
        }
    }


    public void Refresh()
    {
        if (slots.Count == inventory.slots.Count)
        {
            for (int i = 0; i < slots.Count; i++)
            {


                if (inventory.slots[i].itemName != "")
                {
                    slots[i].Setitem(inventory.slots[i]);
                }
                else
                {
                    slots[i].SetEmpty();
                }
            }
        }

    }

    public void MoveToToolbar()
    {
        if (selectedSlot == null || inventory == null || toolbar == null)
        {

            return;
        }       

        Inventory.Slot slot = inventory.slots[selectedSlot.slotID];

        if (slot.IsEmpty)
        {

            return;
        }

        Item item = GameManager.instance.itemManager.GetItemByName(slot.itemName);

        if (item == null || ownerPlayer == null || toolbarUI == null)
        {
            
            return;
        }

        if(!toolbar.Add(item))
        {
            NotificationPopup_UI.Show("Toolbar is full.");
            return;
        }        

        inventory.Remove(selectedSlot.slotID);
        toolbarUI.Refresh();
        Refresh();
    }
    public void Remove()
    {
        Slot_UI slotToRemove = draggedSlot != null ? draggedSlot : selectedSlot;

        if (slotToRemove == null)
        {
            
            return;
        }

        if (inventory == null)
        {
            
            return;
        }

        if (inventory.slots[slotToRemove.slotID].itemName == "")
        {
            return;
        }

        Item itemToDrop = GameManager.instance.itemManager.GetItemByName(
            inventory.slots[slotToRemove.slotID].itemName);

        if (itemToDrop != null)
        {
            if (dragSingle)
            {
                ownerPlayer.DropItem(itemToDrop);
                inventory.Remove(slotToRemove.slotID);
            }
            else
            {
                int amount = inventory.slots[slotToRemove.slotID].count;

                ownerPlayer.DropItem(itemToDrop, amount);
                inventory.Remove(slotToRemove.slotID, amount);
            }

            Refresh();
        }
    }

    public void SlotBeginDrag(Slot_UI slot)
    {
        Debug.Log("BEGIN DRAG: " + slot.slotID);

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
        Slot_UI sourceSlot = draggedSlot;
        
        if (sourceSlot == null && toolbarUI != null)
        {
            sourceSlot = toolbarUI.GetDraggedSlot();
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

        Refresh();

        if (toolbarUI != null)
        {
            toolbarUI.Refresh();
            toolbarUI.ClearDraggedSlot();
        }

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

    void SetupSlots()
    {
        int counter = 0;
        foreach (Slot_UI slot in slots)
        {
            slot.slotID = counter;
            counter++;
            slot.inventory = inventory;
        }
    }

    private int ToolbarToBackpackColumn(
    int toolbarIndex)
    {
        int toolbarCount =
            toolbarUI.ToolbarSlots.Count;

        if (toolbarCount <= 1)
        {
            return 0;
        }

        return Mathf.RoundToInt(
            toolbarIndex *
            (columns - 1f) /
            (toolbarCount - 1f)
        );
    }

    private int BackpackToToolbarIndex(
        int backpackIndex)
    {
        int toolbarCount =
            toolbarUI.ToolbarSlots.Count;

        int backpackColumn =
            backpackIndex % columns;

        if (toolbarCount <= 1)
        {
            return 0;
        }

        return Mathf.RoundToInt(
            backpackColumn *
            (toolbarCount - 1f) /
            (columns - 1f)
        );
    }

    private void RefreshSelector()
    {
        ClearSelectors();

        if (currentArea == InventoryArea.Toolbar)
        {
            toolbarUI.ToolbarSlots[
                currentToolbarSlot
            ].SetSelector(true);
        }
        else
        {
            selectedSlot = slots[currentSlot];
            selectedSlot.SetSelector(true);
        }
    }

    private void ClearSelectors()
    {
        foreach (Slot_UI slot in slots)
        {
            slot.SetSelector(false);
        }

        foreach (Slot_UI slot in toolbarUI.ToolbarSlots)
        {
            slot.SetSelector(false);
        }
    }
    private void TransferSelectedItem()
    {
        Inventory sourceInventory;
        Inventory destinationInventory;
        int sourceIndex;

        if (currentArea == InventoryArea.Toolbar)
        {
            sourceInventory = toolbar;
            destinationInventory = inventory;
            sourceIndex = currentToolbarSlot;
        }
        else
        {
            sourceInventory = inventory;
            destinationInventory = toolbar;
            sourceIndex = currentSlot;
        }

        Inventory.Slot sourceSlot =
            sourceInventory.slots[sourceIndex];

        if (sourceSlot.IsEmpty)
        {
            return;
        }

        Item item = GameManager.instance.itemManager
            .GetItemByName(sourceSlot.itemName);

        if (item == null)
        {
            return;
        }

        // Add() chooses the first matching stack
        // or first empty destination slot.
        if (!destinationInventory.Add(item))
        {
            string message = currentArea == InventoryArea.Toolbar
                ? "Backpack is full."
                : "Toolbar is full.";

            NotificationPopup_UI.Show(message);
            return;
        }

        sourceInventory.Remove(sourceIndex);

        toolbarUI.Refresh();
        Refresh();
    }
}
