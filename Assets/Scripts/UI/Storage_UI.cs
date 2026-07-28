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
    private float nextMoveTime;

    private const int columns = 5;

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

        SelectSlot(0);
        Refresh();
    }

    public void Close()
    {
        storagePanel.SetActive(false);

        openedStorage = null;
        if (activePlayer != null)
        {
            activePlayer.GetComponent<PlayerMovement>().SetMovementLocked(false);
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

        //put one selected toolbar item into selected storage slot
        if(Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            DepositSelectedToolbarItem();
        }

        //take one selected storage item into the players backpack
        if(Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            WithdrawSelectedStorageItem();
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

        if(Keyboard.current.fKey.wasPressedThisFrame)
        {
            DepositSelectedToolbarItem();
        }

        if(Keyboard.current.qKey.wasPressedThisFrame)
        {
            WithdrawSelectedStorageItem();
        }
    }

    private void MoveRight()
    {
        if(currentSlot % columns < columns - 1 &&
            currentSlot + 1 < slots.Count)
        {
            SelectSlot(currentSlot + 1);
            nextMoveTime = Time.time + moveDelay;
        }
    }
    private void MoveLeft()
    {
        if (currentSlot % columns > 0)
        {
            SelectSlot(currentSlot - 1);
            nextMoveTime = Time.time + moveDelay;
        }
    }
    private void MoveUp()
    {
        if (currentSlot - columns >= 0)
        {
            SelectSlot(currentSlot - columns);
            nextMoveTime = Time.time + moveDelay;
        }
    }
    private void MoveDown()
    {
        if (currentSlot + columns < slots.Count)
        {
            SelectSlot(currentSlot + columns);
            nextMoveTime = Time.time + moveDelay;
        }
    }

    private void SelectSlot(int index)
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

    private void DepositSelectedToolbarItem()
    {
        Inventory toolbar = activePlayer.inventoryManager.toolbar;
        int toolbarIndex = toolbar.selectedSlotIndex;

        Inventory.Slot toolbarSlot = toolbar.slots[toolbarIndex];

        if(toolbarSlot.IsEmpty)
        {
            return;
        }

        Item item = GameManager.instance.itemManager.GetItemByName(toolbarSlot.itemName);

        if (item == null || item.data.itemType != ItemData.ItemType.Ingredient)
        {
            Debug.Log("only ingredients can be stored there");
            return;
        }

        int storageIndex =
            FindAvailableSlot(storageInventory, item);

        if(storageIndex == -1)
        {
            Debug.Log("Ingredient storage is full");
            return;
        }

        toolbar.Moveslot(toolbarIndex, storageIndex, storageInventory);
        

        activePlayer.toolbarUI.Refresh();
        Refresh();
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
            Debug.Log("Toolbar and backpack are full");
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
