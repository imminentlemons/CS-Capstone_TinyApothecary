using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Potion Recipe", menuName = "Tiny Apothecary/Potion Recipe")]
public class PotionRecipe : ScriptableObject
{
    [Serializable]
    public class IngredientRequirement
    {
        public ItemData ingredient;
        [Min(1)] public int amount = 1;
    }

    [Header("Result")]
    public ItemData potion;

    [Header("Ingredients")]
    public List<IngredientRequirement> ingredients = new();

    [Header("Brewing")]
    [Min(0.1f)] public float brewDuration = 5f;
    public Sprite brewingCauldronSprite;
    
}
