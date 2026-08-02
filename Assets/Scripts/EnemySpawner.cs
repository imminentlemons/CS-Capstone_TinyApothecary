using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Timing")]
    [SerializeField, Min(0f)]
    private float firstSpawnDelay = 60f;

    [SerializeField, Min(0f)]
    private float minimumSpawnDelay = 40f;

    [SerializeField, Min(0f)]
    private float maximumSpawnDelay = 70f;

    [Header("Enemies")]
    [SerializeField]
    private GameObject[] enemyPrefabs;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform[] spawnPoints;

    [Header("Day Cycle")]
    [SerializeField]
    private DayCycleManager dayCycle;

    [Header("Garden")]
    [SerializeField]
    private Transform gardenTarget;

    [SerializeField]
    private TileManager tileManager;

    private GameObject activeEnemy;
    private Coroutine spawnRoutine;
    private bool spawningEnabled;

    private void OnEnable()
    {
        if (dayCycle != null)
        {
            dayCycle.ShopOpened += HandleShopOpened;
            dayCycle.ShopClosed += HandleShopClosed;
        }
    }

    private void Start()
    {
        // Supports testing when the scene begins
        // during the Open phase.
        if (dayCycle != null && dayCycle.IsShopOpen)
        {
            StartSpawning(firstSpawnDelay);
        }
    }

    private void Update()
    {
        if (!spawningEnabled ||
            activeEnemy != null ||
            spawnRoutine != null ||
            dayCycle == null ||
            !dayCycle.IsShopOpen)
        {
            return;
        }

        float shortestDelay =
            Mathf.Min(minimumSpawnDelay, maximumSpawnDelay);

        float longestDelay =
            Mathf.Max(minimumSpawnDelay, maximumSpawnDelay);

        float delay =
            Random.Range(shortestDelay, longestDelay);

        ScheduleSpawn(delay);
    }

    private void HandleShopOpened()
    {
        StartSpawning(firstSpawnDelay);
    }

    private void HandleShopClosed()
    {
        StopSpawning();

        // The current enemy is deliberately left alive
        // for players to finish during the closing period.
    }

    private void StartSpawning(float delay)
    {
        spawningEnabled = true;
        ScheduleSpawn(delay);
    }

    private void StopSpawning()
    {
        spawningEnabled = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private void ScheduleSpawn(float delay)
    {
        if (!spawningEnabled ||
            activeEnemy != null ||
            spawnRoutine != null ||
            dayCycle == null ||
            !dayCycle.IsShopOpen)
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

        if (!spawningEnabled ||
            activeEnemy != null ||
            dayCycle == null ||
            !dayCycle.IsShopOpen)
        {
            yield break;
        }

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null ||
            enemyPrefabs.Length == 0 ||
            spawnPoints == null ||
            spawnPoints.Length == 0 ||
            gardenTarget == null ||
            tileManager == null)
        {
            return;
        }

        GameObject enemyPrefab =
            enemyPrefabs[
                Random.Range(0, enemyPrefabs.Length)
            ];

        Transform spawnPoint =
            spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

        if (enemyPrefab == null || spawnPoint == null)
        {
            return;
        }

        activeEnemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        EnemyGardenAttack gardenAttack =
            activeEnemy.GetComponent<EnemyGardenAttack>();

        if (gardenAttack != null)
        {
            gardenAttack.Initialize(
                tileManager,
                gardenTarget
            );
        }
    }

    private void OnDisable()
    {
        if (dayCycle != null)
        {
            dayCycle.ShopOpened -= HandleShopOpened;
            dayCycle.ShopClosed -= HandleShopClosed;
        }

        StopSpawning();
    }
}