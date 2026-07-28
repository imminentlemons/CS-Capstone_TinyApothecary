using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Day Cycle")]
    [SerializeField] private DayCycleManager dayCycle;

    private GameObject activeEnemy;
    private bool spawnedThisDefensePhase;

    private void OnEnable()
    {
        if (dayCycle == null)
        {
            return;
        }

        dayCycle.ShopClosed += HandleShopClosed;
        
    }

    private void Start()
    {
        // works if testing begins during  Closed phase
        if (dayCycle != null &&
            dayCycle.CurrentPhase == DayPhase.Closed)
        {
            SpawnFirstEnemy();
        }
    }

    private void HandleShopClosed()
    {
        SpawnFirstEnemy();
    }

    private void SpawnFirstEnemy()
    {
        if (spawnedThisDefensePhase ||
            activeEnemy != null ||
            enemyPrefab == null ||
            spawnPoint == null)
        {
            return;
        }

        spawnedThisDefensePhase = true;

        activeEnemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Defense phase started: slime spawned.");
    }   

    private void OnDisable()
    {
        if (dayCycle == null)
        {
            return;
        }

        dayCycle.ShopClosed -= HandleShopClosed;       
    }
}