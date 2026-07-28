using System;
using UnityEngine;

[Serializable]
public class CustomerOrder
{
    public PotionRecipe recipe;
    public int reward;
    public float patienceSeconds;

    public float remainingPatience;

    public ItemData RequestedPotion =>
        recipe != null ? recipe.potion : null;

    public CustomerOrder(PotionRecipe recipe, int reward, float patienceSeconds)
    {
        this.recipe = recipe;
        this.reward = reward;
        this.patienceSeconds = patienceSeconds; ;
        remainingPatience = patienceSeconds;
    }

    public bool isCorrectPotion(Item item)
    {
        return item != null &&
            RequestedPotion != null &&
            item.data.itemName == RequestedPotion.itemName;
    }
}

