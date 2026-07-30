using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [Header("Wave Settings")]
    [SerializeField, Min(1)]
    private int maximumEnemiesPerWave = 2;

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

    private readonly List<GameObject> activeEnemies =
        new List<GameObject>();

    private bool spawnedThisDefensePhase;

    private void OnEnable()
    {
        if (dayCycle != null)
        {
            dayCycle.ShopClosed += HandleShopClosed;
        }
    }

    private void Start()
    {
        // Supports testing when the scene starts Closed.
        if (dayCycle != null &&
            dayCycle.CurrentPhase == DayPhase.Closed)
        {
            SpawnDefenseEnemies();
        }
    }

    private void HandleShopClosed()
    {
        SpawnDefenseEnemies();
    }

    private void SpawnDefenseEnemies()
    {
        if (spawnedThisDefensePhase ||
            enemyPrefabs == null ||
            enemyPrefabs.Length == 0 ||
            spawnPoints == null ||
            spawnPoints.Length == 0 ||
            gardenTarget == null ||
            tileManager == null)
        {
            return;
        }

        spawnedThisDefensePhase = true;

        int previousSpawnIndex = -1;

        int enemiesToSpawn = Mathf.Min(maximumEnemiesPerWave, enemyPrefabs.Length);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[i];

            int spawnIndex =
                ChooseSpawnIndex(previousSpawnIndex);

            previousSpawnIndex = spawnIndex;

            Transform selectedSpawnPoint =
                spawnPoints[spawnIndex];

            if (selectedSpawnPoint == null)
            {
                continue;
            }

            GameObject enemy = Instantiate(
                enemyPrefab,
                selectedSpawnPoint.position,
                selectedSpawnPoint.rotation
            );

            EnemyGardenAttack gardenAttack =
                enemy.GetComponent<EnemyGardenAttack>();

            if (gardenAttack != null)
            {
                gardenAttack.Initialize(
                    tileManager,
                    gardenTarget
                );
            }

            activeEnemies.Add(enemy);

            Debug.Log(
                $"{enemy.name} spawned at " +
                $"{selectedSpawnPoint.name}."
            );
        }
    }

    private int ChooseSpawnIndex(
        int previousSpawnIndex)
    {
        int index =
            Random.Range(0, spawnPoints.Length);

        // avoids putting consecutive enemies at
        // the same location when alternatives exist
        if (spawnPoints.Length > 1 &&
            index == previousSpawnIndex)
        {
            int offset =
                Random.Range(1, spawnPoints.Length);

            index =
                (previousSpawnIndex + offset) %
                spawnPoints.Length;
        }

        return index;
    }

    private void OnDisable()
    {
        if (dayCycle != null)
        {
            dayCycle.ShopClosed -= HandleShopClosed;
        }
    }
}