using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class SeedShop : MonoBehaviour
{    
    [SerializeField] private ShopFunds shopFunds;
    [SerializeField] private List<Item> seedsForSale = new();

    [Header("Shop Display")]
    [SerializeField] private List<Image> seedIcons = new();

    [SerializeField] private Image cropToGrowIcon;
    [SerializeField] private TMP_Text seedNameText;
    [SerializeField] private TMP_Text seedFlavorText;
    [SerializeField] private TMP_Text priceText;

    [Header("Shop Navigation")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private List<GameObject> seedSelector = new();
    [SerializeField] private GameObject buyButtonSelector;
    [SerializeField] private float moveDelay = 0.2f;

    private enum ShopFocus
    {
        SeedGrid,
        BuyButton
    }

    private const int GridColumns = 3;

    private ShopFocus currentFocus;
    private Player activePlayer;
    private int selectedSeedIndex;
    private float nextMoveTime;
    private int openedFrame;

    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    public IReadOnlyList<Item> SeedsForSale => seedsForSale;

    private void Start()
    {
        PopulateSeedIcons();
        ShowSeedDetails(0);
        HideAllSelectors();

        if(shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameFlow_UI.GameplayUIInputBlocked)
        {
            return;
        }

        if (!IsOpen || activePlayer == null)
        {
            return;
        }

        if (Time.frameCount == openedFrame)
        {
            return;
        }

        Gamepad gamepad = GetActiveGamePad();

        if(gamepad ==  null)
        {
            return;
        }

        if(currentFocus == ShopFocus.SeedGrid)
        {
            CheckSeedGridControls(gamepad);
        }
        else
        {
            CheckBuyButtonControls(gamepad);
        }
    }

    public bool TryBuy(Player buyer, int seedIndex)
    {
        //only p2 can buy seeds
        if(buyer == null || buyer.toolbarUI.inputType != Toolbar_UI.InputType.Gamepad)
        {
            return false;
        }

        if(seedIndex < 0 || seedIndex >= seedsForSale.Count)
        {
            NotificationPopup_UI.Show(buyer, "Select a seed.");
            return false;
        }

        Item seed = seedsForSale[seedIndex];

        if(seed == null)
        {
            NotificationPopup_UI.Show(buyer, "Select a seed.");
            return false;
        }

        if(!HasRoom(buyer.inventoryManager.toolbar, seed) &&
            !HasRoom(buyer.inventoryManager.backpack, seed))
        {
            NotificationPopup_UI.Show(buyer, "Backpack is full.");
            return false;
        }

        if(!shopFunds.TrySpend(seed.data.price))
        {
            NotificationPopup_UI.Show(buyer, "Not enough money.");
            return false;
        }

        buyer.inventoryManager.AddToToolbarThenBackpack(seed);

        buyer.toolbarUI.Refresh();
        buyer.inventoryUI.Refresh();

        AudioManager.PlaySellBuyItem();

        Debug.Log("Bought " + seed.data.itemName);
        return true;
    }

    private bool HasRoom(Inventory inventory, Item item)
    {
        foreach(Inventory.Slot slot in inventory.slots)
        {
            if(slot.IsEmpty || slot.CanAdditem(item.data.itemName))
            {
                return true;
            }
        }

        return false;
    }

    private void PopulateSeedIcons()
    {
        for(int i = 0; i < seedIcons.Count; i++)
        {
            if(i >= seedsForSale.Count ||
                seedsForSale[i] == null ||
                seedsForSale[i].data == null)
            {
                seedIcons[i].sprite = null;
                seedIcons[i].enabled = false;
                continue;
            }

            seedIcons[i].sprite = seedsForSale[i].data.icon;
            seedIcons[i].enabled = true;
            seedIcons[i].preserveAspect = true;
        }
    }

    private void ShowSeedDetails(int seedIndex)
    {
        if(seedIndex < 0 || seedIndex >= seedsForSale.Count)
        {
            return;
        }

        Item seed = seedsForSale[seedIndex];

        if(seed == null || seed.data == null)
        {
            return;
        }

        ItemData seedData = seed.data;

        seedNameText.text = seedData.itemName;
        seedFlavorText.text = seedData.flavorText;
        priceText.text = $"{seedData.price} coins";

        if(seedData.cropToGrow != null)
        {
            cropToGrowIcon.sprite = seedData.cropToGrow.icon;
            cropToGrowIcon.enabled = true;
            cropToGrowIcon.preserveAspect = true;
        }
        else
        {
            cropToGrowIcon.sprite = null;
            cropToGrowIcon.enabled = false;
        }
    }

    public void Open(Player player)
    {
        if(player == null ||
            player.toolbarUI.inputType != Toolbar_UI.InputType.Gamepad)
        {
            return;
        }

        activePlayer = player;

        PlayerMovement movement =
            activePlayer.GetComponent<PlayerMovement>();

        if(movement != null)
        {
            movement.SetMovementLocked(true);
        }

        //prevent b from also moving otolbar items while shopping
        activePlayer.toolbarUI.enabled = false;

        shopPanel.SetActive(true);
        shopPanel.transform.SetAsLastSibling();

        selectedSeedIndex = 0;
        currentFocus = ShopFocus.SeedGrid;
        nextMoveTime = 0f;
        openedFrame = Time.frameCount;

        ShowSeedDetails(selectedSeedIndex);
        RefreshSelectors();
    }

    public void Close()
    {
        AudioManager.PlayCloseUI();
        shopPanel.SetActive(false);
        HideAllSelectors();

        if(activePlayer != null)
        {
            PlayerMovement movement = 
                activePlayer.GetComponent<PlayerMovement>();

            if(movement != null)
            {
                movement.SetMovementLocked(false);
            }

            activePlayer.toolbarUI.enabled = true;
        }

        activePlayer = null;
    }

    private Gamepad GetActiveGamePad()
    {
        if(activePlayer == null)
        {
            return null;
        }

        PlayerInput playerInput =
            activePlayer.GetComponent<PlayerInput>();

        if(playerInput == null)
        {
            return null;
        }

        foreach(InputDevice device in playerInput.devices)
        {
            if(device is Gamepad gamepad)
            {
                return gamepad;
            }
        }
        return null;
    }

    private void CheckSeedGridControls(Gamepad gamepad)
    {
        if(Time.time >= nextMoveTime)
        {
            Vector2 navigation = gamepad.leftStick.ReadValue();
            Vector2 dpad = gamepad.dpad.ReadValue();

            //use whichever input is stronger
            if(dpad.sqrMagnitude > navigation.sqrMagnitude)
            {
                navigation = dpad;
            }

            if (navigation.x > 0.5f)
            {
                MoveRight();
                nextMoveTime = Time.time + moveDelay;
            }
            else if (navigation.x < -0.5f)
            {
                MoveLeft();
                nextMoveTime = Time.time + moveDelay;
            }
            else if (navigation.y > 0.5f)
            {
                MoveUp();
                nextMoveTime = Time.time + moveDelay;
            }
            else if (navigation.y < -0.5f)
            {
                MoveDown();
                nextMoveTime = Time.time + moveDelay;
            }
        }

        if(gamepad.buttonSouth.wasPressedThisFrame)
        {
            FocusBuyButton();
        }

        //b closes shop while selecting seeds
        if(gamepad.buttonEast.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void MoveRight()
    {
        int column = selectedSeedIndex % GridColumns;

        if (column < GridColumns - 1 &&
            selectedSeedIndex + 1 < seedsForSale.Count)
        {
            SelectSeed(selectedSeedIndex + 1);
        }
    }

    private void MoveLeft()
    {
        int column = selectedSeedIndex % GridColumns;

        if (column > 0)
        {
            SelectSeed(selectedSeedIndex - 1);
        }
    }

    private void MoveUp()
    {
        if (selectedSeedIndex - GridColumns >= 0)
        {
            SelectSeed(selectedSeedIndex - GridColumns);
        }
    }

    private void MoveDown()
    {
        if (selectedSeedIndex + GridColumns < seedsForSale.Count)
        {
            SelectSeed(selectedSeedIndex + GridColumns);
        }
    }

    private void SelectSeed(int index)
    {
        if(index < 0 || index >= seedsForSale.Count)
        {
            return;
        }

        selectedSeedIndex = index;

        ShowSeedDetails(selectedSeedIndex);
        RefreshSelectors();
    }

    private void FocusBuyButton()
    {
        currentFocus = ShopFocus.BuyButton;
        RefreshSelectors();
    }

    private void CheckBuyButtonControls(Gamepad gamepad)
    {
        //every a press attempts to buy one seed
        if(gamepad.buttonSouth.wasPressedThisFrame)
        {
            TryBuy(activePlayer, selectedSeedIndex);
        }

        //b returns to seed grid
        if(gamepad.buttonEast.wasPressedThisFrame)
        {
            currentFocus = ShopFocus.SeedGrid;
            RefreshSelectors();
        }
    }

    private void RefreshSelectors()
    {
        for(int i = 0; i < seedSelector.Count; i++)
        {
            if(seedSelector[i] != null)
            {
                bool shouldShow = 
                    currentFocus == ShopFocus.SeedGrid &&
                    i == selectedSeedIndex;

                seedSelector[i].SetActive(shouldShow);
            }
        }

        if(buyButtonSelector != null)
        {
            buyButtonSelector.SetActive(currentFocus == ShopFocus.BuyButton);
        }
    }  
    
    private void HideAllSelectors()
    {
        foreach(GameObject selector in seedSelector)
        {
            if(selector != null)
            {
                selector.SetActive(false);
            }
        }

        if(buyButtonSelector != null)
        {
            buyButtonSelector.SetActive(false);
        }
    }
}
