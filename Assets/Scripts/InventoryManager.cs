using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    [System.Serializable]
    private class StartingItem
    {
        public Item itemPrefab;

        [Min(1)]
        public int amount = 1;
    }

    public Dictionary<string, Inventory> inventoryByName = new Dictionary<string, Inventory>();
    public Inventory_UI inventoryUI;

    [Header("Backpack")]
    public Inventory backpack;
    public int backpackSlotCount;

    [Header("Toolbar")]
    public Inventory toolbar;
    public int toolbarSlotCount;

    [Header("Starting Items")]
    [SerializeField]
    private List<StartingItem> startingItems = new();


    private void Awake()
    {
        backpack = new Inventory(backpackSlotCount);
        toolbar = new Inventory(toolbarSlotCount);

        inventoryByName.Add("Backpack", backpack);
        inventoryByName.Add("Toolbar", toolbar);

        AddStartingItems();
        
    }

    private void AddStartingItems()
    {
        foreach (StartingItem startingItem in startingItems)
        {
            if (startingItem == null ||
                startingItem.itemPrefab == null ||
                startingItem.itemPrefab.data == null)
            {
                continue;
            }

            int amount = Mathf.Max(1, startingItem.amount);

            for (int i = 0; i < amount; i++)
            {
                bool added =
                    AddToToolbarThenBackpack(
                        startingItem.itemPrefab
                    );

                if (!added)
                {
                    Debug.LogWarning(
                        $"Could not add all starting copies of " +
                        $"{startingItem.itemPrefab.data.itemName}. " +
                        "The starting inventory is full."
                    );

                    break;
                }
            }
        }
    }
    public void Add(string inventoryName, Item item)
    {
        if (inventoryByName.ContainsKey(inventoryName))
        {
            inventoryByName[inventoryName].Add(item);

            if (inventoryUI != null)
            {
                inventoryUI.Refresh();
            }
        }
    }

    public Inventory GetInventoryByName(string inventoryName)
    {


        foreach (var pair in inventoryByName)
        {

        }

        if (inventoryByName.ContainsKey(inventoryName))
        {

            return inventoryByName[inventoryName];
        }


        return null;
    }

    public bool AddToToolbarThenBackpack(Item item)
    {
        if(toolbar.Add(item))
        {
            return true;
        }

        return backpack.Add(item);
    }
}
