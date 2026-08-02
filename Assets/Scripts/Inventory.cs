using System.Diagnostics.Contracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Inventory
{
    [System.Serializable]
    public class Slot
    {
        public string itemName;
        public int count;
        public int maxAllowed;

        public Sprite icon;
        
        public Slot()
        {
            itemName = "";
            count = 0;
            maxAllowed = 10;
        }

        public bool IsEmpty
        {
            get
            {
                if(itemName == "" && count == 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool CanAdditem(string itemName)
        {
            if (this.itemName == itemName && count < maxAllowed) 
            {
                return true;
            }

            return false;
        }

        public void AddItem(string itemName, Sprite icon, int maxAllowed) 
        {
            this.itemName = itemName;
            this.icon = icon;
            count++;
            this.maxAllowed = maxAllowed;
        }

        public void AddItem(Item item)
        {
            this.itemName = item.data.itemName;
            this.icon = item.data.icon;
            count++;
        }



        public void RemoveItem()
        {
            if (count >0)
            {
                count--;

                if(count == 0)
                {
                    icon = null;
                    itemName = "";
                }
            }
        }
    }


    public List<Slot> slots = new();
    public Slot selectedSlot = null;
    public int selectedSlotIndex = 0;

    public Inventory(int numSlots)
    { 
        for(int i = 0; i < numSlots; i++)
        {
            Slot slot = new Slot();
            slots.Add(slot);
        }    
    }

    public bool Add(Item item)
    { 
        foreach(Slot slot in slots)
        {
            if(slot.itemName == item.data.itemName &&
                slot.CanAdditem(item.data.itemName))
            {
                slot.AddItem(item);
                return true;
            }
        }

        foreach(Slot slot in slots)
        {
            if(slot.IsEmpty)
            {
                slot.AddItem(item);
                return true;
            }
        }

        return false;
    }

    public void Remove(int index)
    {
        slots[index].RemoveItem();
    }

    public void Remove(int index, int numToRemove)
    {
        if (slots[index].count >= numToRemove)
        {
            for(int i = 0; i< numToRemove; i++)
            {
                Remove(index);
            }
        }
    }

    public void Moveslot(int fromIndex, int toIndex, Inventory toInventory)
    {
        Slot fromSlot = slots[fromIndex];
        Slot toSlot = toInventory.slots[toIndex];

        if(toSlot.IsEmpty || toSlot.CanAdditem(fromSlot.itemName))
        {
            toSlot.AddItem(fromSlot.itemName, fromSlot.icon, fromSlot.maxAllowed);
            fromSlot.RemoveItem();
        }
    }

    public void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        selectedSlot = slots[index];
        if (index >= 0 && index < slots.Count)
        {
            selectedSlot = slots[index];
        }
    }

    public void MoveOrSwapStack(int fromIndex, int toIndex, Inventory toInventory)
    {
        if (toInventory == null ||
            fromIndex < 0 ||
            fromIndex >= slots.Count ||
            toIndex < 0 ||
            toIndex >= toInventory.slots.Count)
        {
            return;
        }

        if(this == toInventory && fromIndex == toIndex)
        {
            return;
        }

        Slot fromSlot = slots[fromIndex];
        Slot toSlot = toInventory.slots[toIndex];

        if(fromSlot.IsEmpty)
        {
            return;
        }

        //move entire stack into an empty slot
        if(toSlot.IsEmpty)
        {
            CopySlot(fromSlot, toSlot);
            ClearSlot(fromSlot);
            return;
        }

        //combine matching stacks where possible
        if(fromSlot.itemName == toSlot.itemName)
        {
            int availableSpace =
                toSlot.maxAllowed - toSlot.count;

            int amountToMove =
                Mathf.Min(fromSlot.count, availableSpace);

            toSlot.count += amountToMove;
            fromSlot.count -= amountToMove;

            if(fromSlot.count == 0)
            {
                ClearSlot(fromSlot);
            }

            return;
        }

        //different occupied items: swap their contents
        string oldName = toSlot.itemName;
        int oldCount = toSlot.count;
        int oldMaximum = toSlot.maxAllowed;
        Sprite oldIcon = toSlot.icon;

        CopySlot(fromSlot, toSlot);

        fromSlot.itemName = oldName;
        fromSlot.count = oldCount;
        fromSlot.maxAllowed = oldMaximum;
        fromSlot.icon = oldIcon;
    }

    private static void CopySlot(Slot source, Slot destination)
    {
        destination.itemName = source.itemName;
        destination.count = source.count;
        destination.maxAllowed = source.maxAllowed;
        destination.icon = source.icon;
    }

    private static void ClearSlot(Slot slot)
    {
        slot.itemName = "";
        slot.count = 0;
        slot.maxAllowed = 10;
        slot.icon = null;
    }
}
