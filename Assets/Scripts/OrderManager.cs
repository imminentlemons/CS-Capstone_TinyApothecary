using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Order Pool")]
    [SerializeField] private List<PotionRecipe> availableRecipes = new();

    [Header("First Customer")]
    [SerializeField]
    private PotionRecipe firstCustomerRecipe;

    private bool firstOrderAssigned;

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
        if (customer == null)
        {
            return;
        }

        bool assigningFirstOrder =
            !firstOrderAssigned &&
            firstCustomerRecipe != null;

        PotionRecipe recipe;

        if (assigningFirstOrder)
        {
            recipe = firstCustomerRecipe;
        }
        else
        {
            if (availableRecipes == null ||
                availableRecipes.Count == 0)
            {
                Debug.LogWarning(
                    "Order Manager has no available recipes."
                );

                return;
            }

            recipe =
                availableRecipes[
                    Random.Range(0, availableRecipes.Count)
                ];
        }

        if (recipe == null || recipe.potion == null)
        {
            Debug.LogWarning(
                "Order recipe has no potion assigned."
            );

            return;
        }

        if (assigningFirstOrder)
        {
            firstOrderAssigned = true;
        }

        int reward = recipe.potion.price;

        float patience =
            Random.Range(
                minimumPatience,
                maximumPatience
            );

        CustomerOrder order =
            new CustomerOrder(
                recipe,
                reward,
                patience
            );

        customer.SetOrder(order);
    }
}
