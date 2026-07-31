using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Storage_UI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject storagePanel;
    [SerializeField] private List<Slot_UI> slots = new();

    [HeaderAttribute("Controls")]
    [SerializeField] private Toolbar_UI.InputType inputType;
    [SerializeField] private float moveDelay = 0.2f;


    private IngredientStorage openedStorage;
    private Player activePlayer;
    private Inventory storageInventory;

    private Slot_UI selectedSlot;
    private int currentSlot;
    private int currentToolbarSlot;
    private float nextMoveTime;
    private int openedFrame;

    private const int columns = 5;

    private enum StorageArea
    {
        Storage,
        Toolbar
    }

    private StorageArea currentArea;

    public bool IsOpen =>
        storagePanel != null && storagePanel.activeSelf;

    private void Start()
    {
        storagePanel.SetActive(false);
    }

    public void Open(IngredientStorage storage, Player player)
    {
        openedStorage = storage;
        activePlayer = player;
        activePlayer.GetComponent<PlayerMovement>().SetMovementLocked(true);
        storageInventory = storage.inventory;

        SetupSlots();

        storagePanel.SetActive(true);
        storagePanel.transform.SetAsLastSibling();

        currentArea = StorageArea.Storage;
        currentToolbarSlot = activePlayer.toolbarUI.CurrentSlotIndex;
        nextMoveTime = 0f;
        openedFrame = Time.frameCount;

        activePlayer.toolbarUI.enabled = false;

        SelectStorageSlot(0);
        Refresh();
        RefreshSelectors();
    }

    public void Close()
    {
        AudioManager.PlayCloseUI();
        storagePanel.SetActive(false);

        openedStorage = null;
        if (activePlayer != null)
        {
            activePlayer.GetComponent<PlayerMovement>().SetMovementLocked(false);
            activePlayer.toolbarUI.enabled = true;
            ClearSelectors();
            activePlayer.toolbarUI.SetGameplaySelectorVisible(true);
        }
        activePlayer = null;
        storageInventory = null;
    }

    private void Update()
    {
        if(!IsOpen)
        {
            return;
        }

        if (Time.frameCount == openedFrame)
        {
            return;
        }

        if(inputType == Toolbar_UI.InputType.Gamepad)
        {
            CheckGamepadControls();
        }
        else
        {
            CheckKeyboardControls();
        }
    }

    private void CheckGamepadControls()
    {
        if(Gamepad.current == null)
        {
            return;
        }

        if(Time.time >= nextMoveTime)
        {
            Vector2 input = Gamepad.current.leftStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();

            if (dpad.sqrMagnitude > input.sqrMagnitude)
            {
                input = dpad;
            }

            if (input.x > 0.5f)
            {
                MoveRight();
            }
            else if (input.x < -0.5f)
            {
                MoveLeft();
            }
            else if (input.y > 0.5f)
            {
                MoveUp();
            }
            else if (input.y < -0.5f)
            {
                MoveDown();
            }
        }        

        if(Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            TransferSelectedItem();
        }

        if(Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void CheckKeyboardControls()
    {
        if(Keyboard.current == null)
        {
            return;
        }


        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            MoveRight();
        }
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            MoveLeft();
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            MoveUp();
        }
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            MoveDown();
        }

        if(Keyboard.current.eKey.wasPressedThisFrame)
        {
            TransferSelectedItem();
        }

        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void MoveRight()
    {
        if (currentArea == StorageArea.Toolbar)
        {
            if (currentToolbarSlot + 1 < activePlayer.toolbarUI.ToolbarSlots.Count)
            {
                currentToolbarSlot++;
            }
        }
        else if(currentSlot % columns < columns - 1 &&
                currentSlot + 1 < slots.Count)
        {
            currentSlot++;
        }

        FinishNavigation();
    }

    private void MoveLeft()
    {
        if (currentArea == StorageArea.Toolbar)
        {
            if (currentToolbarSlot > 0)
            {
                currentToolbarSlot--;
            }
        }
        else if (currentSlot % columns > 0)
        {
            currentSlot--;
        }

        FinishNavigation();
    }

    private void MoveUp()
    {
        if (currentArea == StorageArea.Toolbar)
        {
            currentArea = StorageArea.Storage;

            int bottomRowStart = ((slots.Count - 1) / columns) * columns;
            currentSlot = Mathf.Min(
                bottomRowStart + ToolbarToStorageColumn(currentToolbarSlot),
                slots.Count - 1);
        }
        else if (currentSlot - columns >= 0)
        {
            currentSlot -= columns;
        }

        FinishNavigation();
    }

    private void MoveDown()
    {
        if (currentArea == StorageArea.Toolbar)
        {
            return;
        }

        if (currentSlot + columns < slots.Count)
        {
            currentSlot += columns;
        }
        else
        {
            currentArea = StorageArea.Toolbar;
            currentToolbarSlot = StorageToToolbarIndex(currentSlot);
        }

        FinishNavigation();
    }

    private void SelectStorageSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return;
        }

        if (selectedSlot != null)
        {
            selectedSlot.SetSelector(false);
        }

        currentSlot = index;
        selectedSlot = slots[index];
        selectedSlot.SetSelector(true);
    }

    private void TransferSelectedItem()
    {
        if (currentArea == StorageArea.Toolbar)
        {
            DepositToolbarItem(currentToolbarSlot);
        }
        else
        {
            WithdrawSelectedStorageItem();
        }
    }

    private void DepositToolbarItem(int toolbarIndex)
    {
        Inventory toolbar = activePlayer.inventoryManager.toolbar;

        Inventory.Slot toolbarSlot = toolbar.slots[toolbarIndex];

        if(toolbarSlot.IsEmpty)
        {
            return;
        }

        Item item = GameManager.instance.itemManager.GetItemByName(toolbarSlot.itemName);

        if (item == null || item.data.itemType != ItemData.ItemType.Ingredient)
        {
            NotificationPopup_UI.Show(activePlayer, "Select an ingredient to store.");
            return;
        }

        int storageIndex =
            FindAvailableSlot(storageInventory, item);

        if(storageIndex == -1)
        {
            NotificationPopup_UI.Show(activePlayer, "Storage is full.");
            return;
        }

        toolbar.Moveslot(toolbarIndex, storageIndex, storageInventory);
        

        activePlayer.toolbarUI.Refresh();
        Refresh();
    }

    private void FinishNavigation()
    {
        nextMoveTime = Time.time + moveDelay;
        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        ClearSelectors();

        if (currentArea == StorageArea.Toolbar)
        {
            activePlayer.toolbarUI.ToolbarSlots[currentToolbarSlot].SetSelector(true);
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

        if (activePlayer == null || activePlayer.toolbarUI == null)
        {
            return;
        }

        foreach (Slot_UI slot in activePlayer.toolbarUI.ToolbarSlots)
        {
            slot.SetSelector(false);
        }
    }

    private int ToolbarToStorageColumn(int toolbarIndex)
    {
        int toolbarCount = activePlayer.toolbarUI.ToolbarSlots.Count;

        if (toolbarCount <= 1)
        {
            return 0;
        }

        return Mathf.RoundToInt(toolbarIndex * (columns - 1f) / (toolbarCount - 1f));
    }

    private int StorageToToolbarIndex(int storageIndex)
    {
        int toolbarCount = activePlayer.toolbarUI.ToolbarSlots.Count;

        if (toolbarCount <= 1)
        {
            return 0;
        }

        int storageColumn = storageIndex % columns;
        return Mathf.RoundToInt(storageColumn * (toolbarCount - 1f) / (columns - 1f));
    }

    private void WithdrawSelectedStorageItem()
    {
        WithdrawStorageItem(currentSlot);
    }

    private void WithdrawStorageItem(int slotIndex)
    {
        Inventory.Slot storageSlot =
            storageInventory.slots[slotIndex];

        if (storageSlot.IsEmpty)
        {
            return;
        }

        Item item = GameManager.instance.itemManager
            .GetItemByName(storageSlot.itemName);

        if(item == null)
        {
            return;
        }

        if (!activePlayer.inventoryManager.AddToToolbarThenBackpack(item))
        {
            NotificationPopup_UI.Show(activePlayer, "Backpack is full.");
            return;
        }

        storageInventory.Remove(slotIndex);

        activePlayer.toolbarUI.Refresh();
        activePlayer.inventoryUI.Refresh();
        Refresh();
    }

    private int FindAvailableSlot(Inventory inventory, Item item)
    {
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            if (inventory.slots[i].IsEmpty ||
                inventory.slots[i].CanAdditem(item.data.itemName))
            {
                return i;
            }
        }

        return -1;
    }

    public void Refresh()
    {
        if (storageInventory == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (storageInventory.slots[i].IsEmpty)
            {
                slots[i].SetEmpty();
            }
            else
            {
                slots[i].Setitem(storageInventory.slots[i]);
            }
        }
    }

    private void SetupSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].slotID = i;
            slots[i].inventory = storageInventory;
            slots[i].storageUI = this;
        }
    }

    public void WithdrawSlotByClick(int slotIndex)
    {
        //mouse withdrawal intended for p1
        if(!IsOpen ||
            inputType != Toolbar_UI.InputType.Keyboard ||
            slotIndex < 0 ||
            slotIndex >= storageInventory.slots.Count)
        {
            return;
        }

        WithdrawStorageItem(slotIndex);
    }
}
