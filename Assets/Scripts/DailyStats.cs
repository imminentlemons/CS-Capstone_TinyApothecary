using System;
using UnityEngine;

public class DailyStats : MonoBehaviour
{
    [SerializeField] private ShopFunds shopFunds;

    public int CustomersServed { get; private set; }
    public int CustomersTimedOut { get; private set; }
    public int MoneyEarnedToday { get; private set; }
    public int EnemiesDefeated { get; private set; }

    private float totalSatisfaction;

    public int TotalCustomers =>
        CustomersServed + CustomersTimedOut;

    public float AverageSatisfaction =>
        TotalCustomers > 0
            ? totalSatisfaction / TotalCustomers
            : 0f;

    public float SatisfactionPercentage =>
        AverageSatisfaction * 100f;

    public event Action StatsChanged;

    private void OnEnable()
    {
        if (shopFunds != null)
        {
            shopFunds.CoinsAdded += HandleCoinsAdded;
        }

        EnemyHealth.EnemyDefeated +=
            HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        if (shopFunds != null)
        {
            shopFunds.CoinsAdded -= HandleCoinsAdded;
        }

        EnemyHealth.EnemyDefeated -=
            HandleEnemyDefeated;
    }

    public void RecordCustomerResult(
        Customer.CustomerOutcome outcome,
        float satisfaction)
    {
        satisfaction = Mathf.Clamp01(satisfaction);

        if (outcome == Customer.CustomerOutcome.Served)
        {
            CustomersServed++;
            totalSatisfaction += satisfaction;
        }
        else
        {
            CustomersTimedOut++;

            // A timed-out customer contributes zero
            // satisfaction to the daily average.
        }

        NotifyStatsChanged();
    }

    private void HandleCoinsAdded(int amount)
    {
        MoneyEarnedToday += amount;
        NotifyStatsChanged();
    }

    private void HandleEnemyDefeated()
    {
        EnemiesDefeated++;
        NotifyStatsChanged();
    }

    public void ResetForNewDay()
    {
        CustomersServed = 0;
        CustomersTimedOut = 0;
        MoneyEarnedToday = 0;
        EnemiesDefeated = 0;
        totalSatisfaction = 0f;

        NotifyStatsChanged();
    }

    private void NotifyStatsChanged()
    {
        Debug.Log(
            $"Daily Stats — Served: {CustomersServed}, " +
            $"Timed Out: {CustomersTimedOut}, " +
            $"Satisfaction: {SatisfactionPercentage:0}%, " +
            $"Earned: {MoneyEarnedToday}, " +
            $"Enemies: {EnemiesDefeated}"
        );

        StatsChanged?.Invoke();
    }
}