using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ShopFunds : MonoBehaviour
{
    [SerializeField] private int coins = 50;

    public int Coins => coins;

    public event Action<int> CoinsChanged;
    public event Action<int> CoinsAdded;

    public void AddCoins(int amount)
    {
        if(amount <= 0)
        {
            return;
        }

        coins += amount;

        CoinsChanged?.Invoke(coins);
        CoinsAdded?.Invoke(amount);
    }

    public bool TrySpend(int amount)
    {
        if(amount <= 0 || coins < amount)
        {
            return false;
        }

        coins -= amount;

        CoinsChanged?.Invoke(coins);
        return true;
    }
}
