using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BrewingStation : MonoBehaviour
{
    [SerializeField] private List<PotionRecipe> recipes = new();

    [SerializeField] private SpriteRenderer cauldronRenderer;
    [SerializeField] private Sprite emptyCauldronSprite;

    [SerializeField] private GameObject timerPanel;
    [SerializeField] private Animator timerAnimator;
    [SerializeField] private AnimationClip timerAnimation;
    [SerializeField] private string timerStateName = "Timer";
    [SerializeField] private GameObject checkmarkObject;

    private bool isBrewing;
    private bool isReady;
    private Item finishedPotion;

    public bool IsBrewing => isBrewing;
    public bool IsReady => isReady;
          

    public IReadOnlyList<PotionRecipe> Recipes => recipes;

    private void Awake()
    {
        if(timerPanel != null)
        {
            timerPanel.SetActive(false);
        }

        if(checkmarkObject != null)
        {
            checkmarkObject.SetActive(false);
        }
    }

    public bool Brew(Player player, PotionRecipe recipe)
    {
        if(isBrewing || isReady)
        {
            Debug.Log("This cauldron is already brewing or ready.");
            return false;
        }

        if (player == null || 
            recipe == null || 
            player.ingredientStorage == null || 
            !recipes.Contains(recipe))
        {
            return false;
        }

        Inventory ingredientInventory = player.inventoryManager.toolbar;

        if (!HasIngredients(ingredientInventory, recipe))
        {
            Debug.Log("Not enough ingredients.");
            return false;
        }

        Item potionItem = GameManager.instance.itemManager
            .GetItemByName(recipe.potion.itemName);

        if (potionItem == null)
        {
            Debug.LogWarning("Potion prefab was not found: " + recipe.potion.itemName);
            return false;
        }

        if (!HasRoom(player.inventoryManager.toolbar, potionItem) &&
            !HasRoom(player.inventoryManager.backpack, potionItem))
        {
            Debug.Log("Toolbar and backpack are full.");
            return false;
        }

        ConsumeIngredients(ingredientInventory, recipe);        
        player.toolbarUI.Refresh();

        if(checkmarkObject != null)
        {
            checkmarkObject.SetActive(false);
        }


        StartCoroutine(BrewRoutine(recipe, potionItem));
        return true;
    }

    private bool HasIngredients(Inventory inventory, PotionRecipe potionRecipe)
    {
        Dictionary<ItemData, int> totals = GetIngredientTotals(potionRecipe);

        foreach(KeyValuePair<ItemData, int> requirement in totals)
        {
            int available = 0;

            foreach(Inventory.Slot slot in inventory.slots)
            {
                if(slot.itemName == requirement.Key.itemName)
                {
                    available += slot.count;
                }
            }

            if(available < requirement.Value)
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeIngredients(Inventory inventory, PotionRecipe potionRecipe)
    {
        Dictionary<ItemData, int> totals = GetIngredientTotals(potionRecipe);

        foreach(KeyValuePair<ItemData, int> requirement in totals)
        {
            int remaining = requirement.Value;

            for (int i = 0; i < inventory.slots.Count && remaining > 0; i++)
            {
                Inventory.Slot slot  = inventory.slots[i];

                while(slot.itemName == requirement.Key.itemName &&
                    slot.count > 0 &&
                    remaining > 0)
                {
                    inventory.Remove(i);
                    remaining--;
                }
            }
        }
    }

    private Dictionary<ItemData, int> GetIngredientTotals(PotionRecipe potionRecipe) 
    {
        Dictionary<ItemData, int> totals = new();
        
        foreach(PotionRecipe.IngredientRequirement requirement in potionRecipe.ingredients)
        {
            if(requirement.ingredient == null)
            {
                continue;
            }

            if(totals.ContainsKey(requirement.ingredient))
            {
                totals[requirement.ingredient] += requirement.amount;
            }
            else
            {
                totals.Add(requirement.ingredient, requirement.amount);
            }
        }

        return totals;
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

    private IEnumerator BrewRoutine(PotionRecipe recipe, Item potionItem)
    {
        isBrewing = true;

        if(cauldronRenderer != null &&
            recipe.brewingCauldronSprite != null)
        {
            cauldronRenderer.sprite = recipe.brewingCauldronSprite;
        }

        ShowTimer(recipe.brewDuration);

        yield return new WaitForSeconds(recipe.brewDuration);

        HideTimer();

        isBrewing = false;
        isReady = true;
        finishedPotion = potionItem;
        
        if(checkmarkObject != null)
        {
            checkmarkObject.SetActive(true);
        }
        
        Debug.Log(recipe.potion.itemName + " is ready to collect.");
    }

    private void ShowTimer(float brewDuration)
    {
        timerPanel.SetActive(true);

        timerAnimator.speed = timerAnimation.length / brewDuration;

        // Starts the animation from frame zero every time.
        timerAnimator.Play("Timer", 0, 0f);
    }

    private void HideTimer()
    {
        timerAnimator.speed = 1f;
        timerPanel.SetActive(false);
    }

    public bool Collect(Player player)
    {
        if(!isReady || finishedPotion == null || player == null)
        {
            return false;
        }

        if(!player.inventoryManager.AddToToolbarThenBackpack(finishedPotion))
        {
            Debug.Log("Make room in the toolbar or backpack first");
            return false;
        }

        player.toolbarUI.Refresh();
        player.inventoryUI.Refresh();

        isReady = false;
        finishedPotion = null;

        if(checkmarkObject != null)
        {
            checkmarkObject.SetActive(false);
        }

        if(cauldronRenderer != null && emptyCauldronSprite != null)
        {
            cauldronRenderer.sprite = emptyCauldronSprite;
        }

        return true;
    }
}
