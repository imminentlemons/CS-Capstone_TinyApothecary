using UnityEngine;

public class IngredientStorage : MonoBehaviour
{
    [SerializeField] private int storageSlots = 20;

    public Inventory inventory { get; private set; }

    private void Awake()
    {
        inventory = new Inventory(storageSlots);
    }

    //returns false when storage is full
    public bool Deposit(Item item)
    {
        foreach(Inventory.Slot slot in inventory.slots)
        {
            if(slot.CanAdditem(item.data.itemName))
            {
                slot.AddItem(item);
                return true;
            }
        }

        foreach(Inventory.Slot slot in inventory.slots)
        {
            if(slot.IsEmpty)
            {
                slot.AddItem(item);
                return true;
            }
        }

        return false;        
    }

    //takes one item from the first non empty storage slot
    public bool Withdraw(out Item item)
    {
        item = null;

        foreach(Inventory.Slot slot in inventory.slots)
        {
            if(slot.IsEmpty)
            {
                continue;
            }

            item = GameManager.instance.itemManager.GetItemByName(slot.itemName);

            if(item == null)
            {
                Debug.LogWarning("Storage cant find prefab for: " + slot.itemName);
                return false;
            }

            slot.RemoveItem();
            return true;
        }

        return false;
    }
}
