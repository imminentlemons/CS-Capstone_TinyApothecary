using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField]
    private Customer[] customerPrefabs;

    [Header("Customer Paths")]
    [FormerlySerializedAs("customerPath")]
    [SerializeField]
    private Transform[] customerPathOne;

    [SerializeField]
    private Transform[] customerPathTwo;

    [Header("Customer Capacity")]
    [SerializeField, Range(1, 2)]
    private int maximumActiveCustomers = 2;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float firstSpawnDelay = 0f;

    [SerializeField, Min(0f)]
    private float customerStaggerDelay = 4f;

    [SerializeField, Min(0f)]
    private float minimumSpawnDelay = 6f;

    [SerializeField, Min(0f)]
    private float maximumSpawnDelay = 10f;

    [Header("Dependencies")]
    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private ShopFunds shopFunds;

    [SerializeField]
    private DayCycleManager dayCycle;

    [SerializeField]
    private DailyStats dailyStats;

    private Customer laneOneCustomer;
    private Customer laneTwoCustomer;

    private Coroutine spawnRoutine;
    private bool spawningEnabled;
    private bool firstCustomerFinished;

    private int ActiveCustomerCount
    {
        get
        {
            int count = 0;

            if (laneOneCustomer != null)
            {
                count++;
            }

            if (laneTwoCustomer != null)
            {
                count++;
            }

            return count;
        }
    }

    private int CurrentCapacity =>
        firstCustomerFinished
            ? maximumActiveCustomers
            : 1;

    private void OnEnable()
    {
        if (dayCycle == null)
        {
            return;
        }

        dayCycle.ShopOpened += HandleShopOpened;
        dayCycle.ShopClosed += HandleShopClosed;
    }

    private void Start()
    {
        // Supports testing when the scene begins
        // during the Open phase.
        if (dayCycle != null && dayCycle.IsShopOpen)
        {
            BeginSpawning(firstSpawnDelay);
        }
    }

    private void HandleShopOpened()
    {
        BeginSpawning(firstSpawnDelay);
    }

    private void HandleShopClosed()
    {
        spawningEnabled = false;
        CancelScheduledSpawn();

        // Existing customers are allowed to finish.
    }

    private void BeginSpawning(float delay)
    {
        spawningEnabled = true;
        ScheduleSpawn(delay);
    }

    private void ScheduleSpawn(float delay)
    {
        if (!CanScheduleCustomer())
        {
            return;
        }

        spawnRoutine =
            StartCoroutine(SpawnAfterDelay(delay));
    }

    private IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        spawnRoutine = null;

        if (!CanSpawnCustomer())
        {
            yield break;
        }

        bool spawned = SpawnCustomer();

        if (spawned &&
            firstCustomerFinished &&
            ActiveCustomerCount < CurrentCapacity)
        {
            ScheduleSpawn(customerStaggerDelay);
        }
    }

    private bool CanScheduleCustomer()
    {
        return spawningEnabled &&
               spawnRoutine == null &&
               dayCycle != null &&
               dayCycle.IsShopOpen &&
               ActiveCustomerCount < CurrentCapacity;
    }

    private bool CanSpawnCustomer()
    {
        return spawningEnabled &&
               dayCycle != null &&
               dayCycle.IsShopOpen &&
               ActiveCustomerCount < CurrentCapacity;
    }

    private bool SpawnCustomer()
    {
        if (customerPrefabs == null ||
            customerPrefabs.Length == 0 ||
            orderManager == null ||
            !TryGetAvailableLane(
                out int laneNumber,
                out Transform[] selectedPath))
        {
            return false;
        }

        Customer selectedPrefab =
            customerPrefabs[
                Random.Range(0, customerPrefabs.Length)
            ];

        if (selectedPrefab == null)
        {
            Debug.LogWarning(
                "Customer prefab array contains an empty slot."
            );

            return false;
        }

        Transform spawnPoint = selectedPath[0];

        Customer customer = Instantiate(
            selectedPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (laneNumber == 1)
        {
            laneOneCustomer = customer;
        }
        else
        {
            laneTwoCustomer = customer;
        }

        customer.ArrivedAtCounter +=
            HandleCustomerArrived;

        customer.Finished +=
            HandleCustomerFinished;

        customer.Resolved +=
            HandleCustomerResolved;

        customer.Initialize(
            shopFunds,
            selectedPath
        );

        return true;
    }

    private bool TryGetAvailableLane(
        out int laneNumber,
        out Transform[] selectedPath)
    {
        laneNumber = 0;
        selectedPath = null;

        if (laneOneCustomer == null &&
            IsValidPath(customerPathOne))
        {
            laneNumber = 1;
            selectedPath = customerPathOne;
            return true;
        }

        if (firstCustomerFinished &&
            maximumActiveCustomers >= 2 &&
            laneTwoCustomer == null &&
            IsValidPath(customerPathTwo))
        {
            laneNumber = 2;
            selectedPath = customerPathTwo;
            return true;
        }

        return false;
    }

    private static bool IsValidPath(Transform[] path)
    {
        return path != null &&
               path.Length > 0 &&
               path[0] != null;
    }

    private void HandleCustomerArrived(
        Customer customer)
    {
        if (!IsTrackedCustomer(customer))
        {
            return;
        }

        orderManager.GiveNewOrder(customer);
    }

    private void HandleCustomerResolved(
        Customer customer,
        Customer.CustomerOutcome outcome,
        float satisfaction)
    {
        if (dailyStats != null)
        {
            dailyStats.RecordCustomerResult(
                outcome,
                satisfaction
            );
        }
    }

    private void HandleCustomerFinished(
        Customer customer,
        Customer.CustomerOutcome outcome,
        float satisfaction)
    {
        UnsubscribeFromCustomer(customer);

        if (laneOneCustomer == customer)
        {
            laneOneCustomer = null;
        }

        if (laneTwoCustomer == customer)
        {
            laneTwoCustomer = null;
        }

        // The first customer must leave completely before
        // the second customer lane becomes available.
        if (!firstCustomerFinished)
        {
            firstCustomerFinished = true;
        }

        if (spawningEnabled &&
            dayCycle != null &&
            dayCycle.IsShopOpen)
        {
            ScheduleSpawn(GetRandomSpawnDelay());
        }
    }

    private bool IsTrackedCustomer(Customer customer)
    {
        return customer != null &&
               (customer == laneOneCustomer ||
                customer == laneTwoCustomer);
    }

    private float GetRandomSpawnDelay()
    {
        float shortestDelay =
            Mathf.Min(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

        float longestDelay =
            Mathf.Max(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

        return Random.Range(
            shortestDelay,
            longestDelay
        );
    }

    private void CancelScheduledSpawn()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private void UnsubscribeFromCustomer(
        Customer customer)
    {
        if (customer == null)
        {
            return;
        }

        customer.ArrivedAtCounter -=
            HandleCustomerArrived;

        customer.Finished -=
            HandleCustomerFinished;

        customer.Resolved -=
            HandleCustomerResolved;
    }

    private void OnDisable()
    {
        if (dayCycle != null)
        {
            dayCycle.ShopOpened -= HandleShopOpened;
            dayCycle.ShopClosed -= HandleShopClosed;
        }

        spawningEnabled = false;
        CancelScheduledSpawn();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCustomer(laneOneCustomer);
        UnsubscribeFromCustomer(laneTwoCustomer);
    }
}