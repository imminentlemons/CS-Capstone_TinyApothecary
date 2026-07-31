using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PotionBook_UI : MonoBehaviour
{
    [Serializable]
    private class IngredientDisplay
    {
        public Image icon;
        public TextMeshProUGUI text;
    }

    [Header("Panel")]
    [SerializeField] private GameObject bookPanel;

    [Header("Recipe Display")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TextMeshProUGUI potionNameText;
    [SerializeField] private TextMeshProUGUI flavorTextLabel;
    [SerializeField] private IngredientDisplay[] ingredientDisplays;

    [Header("Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button brewButton;


    private BrewingStation activeStation;
    private Player activePlayer;
    private int recipeIndex;
    private int openedFrame;

    public bool IsOpen =>
        bookPanel != null && bookPanel.activeSelf;

    private void Start()
    {
        bookPanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen || Time.frameCount == openedFrame || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            PlayPageTurnIfAvailable();
            PreviousRecipe();
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            PlayPageTurnIfAvailable();
            NextRecipe();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            BrewCurrentRecipe();
        }
    }

    public void Open(BrewingStation station, Player player)
    {
        if (station == null || player == null || station.Recipes.Count == 0)
        {
            return;
        }

        activeStation = station;
        activePlayer = player;
        recipeIndex = 0;
        openedFrame = Time.frameCount;

        activePlayer.GetComponent<PlayerMovement>().SetMovementLocked(true);

        bookPanel.SetActive(true);
        bookPanel.transform.SetAsLastSibling();

        Refresh();
    }

    public void Close()
    {
        AudioManager.PlayCloseUI();
        bookPanel.SetActive(false);

        if(activePlayer != null)
        {
            activePlayer.GetComponent<PlayerMovement>().SetMovementLocked(false);
        }

        activeStation = null;
        activePlayer = null;
    }

    public void NextRecipe()
    {
        if(activeStation == null)
        {
            return;
        }

        recipeIndex = (recipeIndex + 1) % activeStation.Recipes.Count;
        Refresh();
    }

    public void PreviousRecipe()
    {
        if(activeStation == null)
        {
            return;
        }

        recipeIndex--;

        if(recipeIndex < 0)
        {
            recipeIndex = activeStation.Recipes.Count - 1;
        }

        Refresh();
    }

    private void PlayPageTurnIfAvailable()
    {
        if(activeStation != null && activeStation.Recipes.Count > 1)
        {
            AudioManager.PlayPageTurn();
        }
    }

    public void BrewCurrentRecipe()
    {
        if(activeStation == null || activePlayer == null)
        {
            return;
        }

        PotionRecipe recipe = activeStation.Recipes[recipeIndex];

        if(activeStation.Brew(activePlayer, recipe))
        {
            Close();
        }
    }

    private void Refresh()
    {
        PotionRecipe recipe = activeStation.Recipes[recipeIndex];

        potionIcon.sprite = recipe.potion.icon;
        potionNameText.text = recipe.potion.itemName;
        flavorTextLabel.text = recipe.potion.flavorText;

        for (int i = 0; i < ingredientDisplays.Length; i++)
        {
            IngredientDisplay display = ingredientDisplays[i];

            if (i < recipe.ingredients.Count)
            {
                PotionRecipe.IngredientRequirement requirement =
                    recipe.ingredients[i];

                display.icon.enabled = true;
                display.icon.sprite = requirement.ingredient.icon;
                display.text.text =
                     requirement.ingredient.itemName;
            }
            else
            {
                display.icon.enabled= false;
                display.text.text = "";
            }
        }

        bool hasMultipleRecipes = activeStation.Recipes.Count > 1;
        previousButton.gameObject.SetActive(hasMultipleRecipes);
        nextButton.gameObject.SetActive(hasMultipleRecipes);
        brewButton.interactable = true;
    }
}
