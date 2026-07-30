using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField] private Customer[] customerPrefabs;
    [SerializeField] private Transform[] customerPath;

    [Header("Dependencies")]
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private ShopFunds shopFunds;
    [SerializeField] private DayCycleManager dayCycle;

    [Header("Timing")]
    [SerializeField] private float minimumSpawnDelay = 3f;
    [SerializeField] private float maximumSpawnDelay = 8f;

    [SerializeField]
    private DailyStats dailyStats;

    private Customer activeCustomer;
    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        if(dayCycle == null)
        {
            return;
        }

        dayCycle.ShopOpened += HandleShopOpened;
        dayCycle.ShopClosed += HandleShopClosed;
    }

    private void Start()
    {
        //allows spawner to work ig scene begins during open phase
        if(dayCycle != null && dayCycle.IsShopOpen)
        {
            StartSpawning(0f);
        }        
    }

    private void StartSpawning(float delay)
    {
        if(spawnRoutine != null ||
            activeCustomer != null ||
            dayCycle == null ||
            !dayCycle.IsShopOpen)
        {
            return;
        }

        spawnRoutine = StartCoroutine(SpawnAfterDelay(delay));
    }

    private IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        spawnRoutine = null;


        //shop may have closed while coroutine was waiting
        if (dayCycle == null || !dayCycle.IsShopOpen)
        {
            yield break;
        }

        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (activeCustomer != null ||
            customerPrefabs == null ||
            customerPrefabs.Length == 0 ||
            customerPath == null ||
            customerPath.Length == 0 ||
            customerPath[0] == null ||
            orderManager == null ||
            dayCycle == null ||
            !dayCycle.IsShopOpen)
        {
            return;
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

            return;
        }

        activeCustomer = Instantiate(
            selectedPrefab,
            customerPath[0].position,
            customerPath[0].rotation
        );

        activeCustomer.ArrivedAtCounter +=
            HandleCustomerArrived;

        activeCustomer.Finished +=
            HandleCustomerFinished;

        activeCustomer.Resolved +=
            HandleCustomerResolved;

        activeCustomer.Initialize(
            shopFunds,
            customerPath
        );
    }

    private void HandleCustomerArrived(Customer customer)
    {
        if(customer != activeCustomer)
        {
            return;
        }

        orderManager.GiveNewOrder(customer);
    }

    private void HandleCustomerFinished(Customer customer, Customer.CustomerOutcome outcome, float satisfaction)
    {
        customer.ArrivedAtCounter -= HandleCustomerArrived;
        
        customer.Finished -= HandleCustomerFinished;

        customer.Resolved -= HandleCustomerResolved;

        if(activeCustomer == customer)
        {
            activeCustomer = null;
        }

        //only schedule another customer while shop is open
        if(dayCycle != null && dayCycle.IsShopOpen)
        {
            float delay = Random.Range(minimumSpawnDelay, maximumSpawnDelay);

            StartSpawning(delay);
        }
        
    }

    private void HandleCustomerResolved( Customer customer, Customer.CustomerOutcome outcome, float satisfaction)
    {
        if(dailyStats != null)
        {
            dailyStats.RecordCustomerResult(outcome, satisfaction);
        }
    }

    private void HandleShopOpened()
    {
        StartSpawning(0f);
    }

    private void HandleShopClosed()
    {
        //prevent customer that was scheduled before closing to appear afterward
        if(spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        //leave activeCustomer alone so current customer can finish transaction
    }

    private void OnDisable()
    {
        if(dayCycle != null)
        {
            dayCycle.ShopOpened -= HandleShopOpened;
            dayCycle.ShopClosed -= HandleShopClosed;
        }

        if(spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private void OnDestroy()
    {
        if(activeCustomer != null)
        {
            activeCustomer.ArrivedAtCounter -=
                HandleCustomerArrived;

            activeCustomer.Finished -=
                HandleCustomerFinished;
            
            activeCustomer.Resolved -=
                HandleCustomerResolved;
        }
    }
}
