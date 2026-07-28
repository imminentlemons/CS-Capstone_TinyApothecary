using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Order Pool")]
    [SerializeField] private List<PotionRecipe> availableRecipes = new();

    [Header("Customers To Start")]
    [SerializeField] private List<Customer> startingCustomers = new();

    [Header("Order Values")]
    
    [SerializeField] private float minimumPatience = 45f;
    [SerializeField] private float maximumPatience = 75f;

    private void Start()
    {
        foreach(Customer customer in startingCustomers)
        {
            GiveNewOrder(customer);
        }
    }

    public void GiveNewOrder(Customer customer)
    {
        if(customer == null || availableRecipes.Count == 0)
        {
            return;
        }

        PotionRecipe recipe = availableRecipes[Random.Range(0, availableRecipes.Count)];

        if (recipe == null || recipe.potion == null)
        {
            Debug.LogWarning("Order recipe has no potion assigned.");
            return;
        }

        int reward = recipe.potion.price;

        float patience = Random.Range(minimumPatience, maximumPatience);

        CustomerOrder order = new CustomerOrder(recipe, reward, patience);

        customer.SetOrder(order);
    }
}
