using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap interactableMap;

    [SerializeField] private Tile hiddenInteractableTile;
    [SerializeField] private Tile plowedTile;
    [SerializeField] private Tile plantedSeedTile;
    [SerializeField] private Tile wateredTile;     

    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap highlightMap;
    [SerializeField] private Tilemap cropMap;

    private Dictionary<Vector3Int, FarmState> farmTiles = new();
    private Dictionary<Vector3Int, ItemData> plantedCrops = new();
    private Dictionary<Vector3Int, GrowthStage> cropStages = new();
    private Dictionary<Vector3Int, float> growthTimers = new();
    public enum FarmState
    {
        Empty,
        Plowed,
        Growing,
        Watered,
        Ready
    }

    public enum GrowthStage
    {
        WateredSeed,
        Seedling,
        Sprout,
        Mature,
        Adult
    }

    void Start()
    {
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(position);

            if (tile != null && tile.name == "Interactable_Visible")
            {
                interactableMap.SetTile(position, hiddenInteractableTile);
            }
        }
    }

    private void Update()
    {
        if (growthTimers.Count == 0)
        {
            return;
        }

        List<Vector3Int> cropPositions = new List<Vector3Int>(growthTimers.Keys);

        foreach (Vector3Int position in cropPositions)
        {
            growthTimers[position] -= Time.deltaTime;

            if (growthTimers[position] <= 0)
            {
                AdvanceGrowth(position);
            }
        }
    }

    private void AdvanceGrowth(Vector3Int position)
    {      

        Debug.Log("Advancing crop at " + position + " from " + cropStages[position]);

        switch (cropStages[position])
        {
            case GrowthStage.WateredSeed:

                cropStages[position] = GrowthStage.Seedling;

                ItemData crop = plantedCrops[position];

                cropMap.SetTile(position, crop.seedlingTile);

                growthTimers[position] = 3f;

                break;


            case GrowthStage.Seedling:

                cropStages[position] = GrowthStage.Sprout;

                cropMap.SetTile(position, plantedCrops[position].sproutTile);

                growthTimers[position] = 3f;

                break;


            case GrowthStage.Sprout:

                cropStages[position] = GrowthStage.Mature;

                cropMap.SetTile(position, plantedCrops[position].matureTile);

                growthTimers[position] = 3f;

                break;


            case GrowthStage.Mature:

                cropStages[position] = GrowthStage.Adult;

                cropMap.SetTile(position, plantedCrops[position].adultTile);

                farmTiles[position] = FarmState.Ready;

                growthTimers.Remove(position);

                break;           

        }
    }

    public FarmState GetFarmState(Vector3Int position)
    {
        if(farmTiles.TryGetValue(position, out FarmState state))
        {
            return state;
        }
        return FarmState.Empty;
    }

    public bool IsFarmTile(Vector3Int position)
    {
        TileBase tile = interactableMap.GetTile(position);

        if(tile == null)
        {
            return false;            
        }

        return tile == hiddenInteractableTile 
            || tile == plowedTile
            || tile == plantedSeedTile
            || tile == wateredTile;
    }    

    public void Plow(Vector3Int position)
    {
        interactableMap.SetTile(position, plowedTile);

        farmTiles[position] = FarmState.Plowed;
    }

    public bool Plant(Vector3Int position, ItemData cropToGrow)
    {

        if(!farmTiles.ContainsKey(position))
        {
            return false;
        }

        if (farmTiles[position] != FarmState.Plowed)
        {
            Debug.Log("Can't plant here");
            return false;
        }                  

        farmTiles[position] = FarmState.Growing;

        plantedCrops[position] = cropToGrow;       

        cropMap.SetTile(position, plantedSeedTile);

        Debug.Log("Planted" + cropToGrow.itemName);

        return true;
        
    }

    public bool Water(Vector3Int position)
    {
        if (!farmTiles.ContainsKey(position))
        {
            return false;
        }

        if (farmTiles[position] != FarmState.Growing)
        {
            return false;
        }

        bool isFirstWatering =
            !cropStages.ContainsKey(position);

        farmTiles[position] = FarmState.Watered;

        if (isFirstWatering)
        {
            cropStages[position] =
                GrowthStage.WateredSeed;
        }

        // Starts or restarts growth from its current stage.
        growthTimers[position] = 3f;

        interactableMap.SetTile(
            position,
            wateredTile
        );

        if (isFirstWatering ||
            cropStages[position] ==
            GrowthStage.WateredSeed)
        {
            cropMap.SetTile(
                position,
                plantedCrops[position].wateredSeedTile
            );
        }

        Debug.Log("Crop Watered");

        return true;
    }

    public bool Harvest(Vector3Int position, out ItemData harvestedItem)
    {
        harvestedItem = null;

        if(!farmTiles.ContainsKey(position) || farmTiles[position] != FarmState.Ready)
        {
            return false;
        }

        harvestedItem = plantedCrops[position];

        cropMap.SetTile(position, null);

        interactableMap.SetTile(position, plowedTile);
        farmTiles[position] = FarmState.Plowed;

        plantedCrops.Remove(position);
        cropStages.Remove(position);
        growthTimers.Remove(position);

        return true;
    }

    private bool CanDamageCrop(Vector3Int position)
    {
        if (!farmTiles.TryGetValue(
                position,
                out FarmState state))
        {
            return false;
        }

        bool validState =
            state == FarmState.Watered ||
            state == FarmState.Ready;

        return validState &&
               cropStages.ContainsKey(position) &&
               plantedCrops.ContainsKey(position);
    }

    public bool TryGetRandomDamageableCrop(out Vector3Int position)
    {
        position = default;

        List<Vector3Int> eligibleCrops =
            new List<Vector3Int>();

        foreach (var crop in farmTiles)
        {
            if (CanDamageCrop(crop.Key))
            {
                eligibleCrops.Add(crop.Key);
            }
        }

        if (eligibleCrops.Count == 0)
        {
            return false;
        }

        position =
            eligibleCrops[
                Random.Range(0, eligibleCrops.Count)
            ];

        return true;
    }

    public bool DamageCrop(Vector3Int position)
    {
        if (!CanDamageCrop(position))
        {
            return false;
        }

        ItemData cropData = plantedCrops[position];

        GrowthStage previousStage =
            cropStages[position];

        GrowthStage damagedStage =
            previousStage;

        TileBase damagedSprite =
            cropMap.GetTile(position);

        switch (previousStage)
        {
            case GrowthStage.Adult:
                damagedStage = GrowthStage.Mature;
                damagedSprite = cropData.matureTile;
                break;

            case GrowthStage.Mature:
                damagedStage = GrowthStage.Sprout;
                damagedSprite = cropData.sproutTile;
                break;

            case GrowthStage.Sprout:
                damagedStage = GrowthStage.Seedling;
                damagedSprite = cropData.seedlingTile;
                break;

            case GrowthStage.Seedling:
            case GrowthStage.WateredSeed:
                damagedStage = GrowthStage.WateredSeed;
                damagedSprite = plantedSeedTile;
                break;
        }

        cropStages[position] = damagedStage;
        farmTiles[position] = FarmState.Growing;

        growthTimers.Remove(position);

        interactableMap.SetTile(
            position,
            plantedSeedTile
        );

        cropMap.SetTile(
            position,
            damagedSprite
        );

        Debug.Log(
            $"Enemy damaged crop at {position}: " +
            $"{previousStage} -> {damagedStage}."
        );

        return true;
    }

    public void HighlightTile(Vector3Int position)
    {
        highlightMap.ClearAllTiles();
        highlightMap.SetTile(position, highlightTile);
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return interactableMap.WorldToCell(worldPosition);
    }

    public void ClearHighlight()
    {
        highlightMap.ClearAllTiles();
    }

    public string GetTileName(Vector3Int position)
    {
        if(interactableMap != null)
        {
            TileBase tile = interactableMap.GetTile(position);

            if( tile != null)
            {
                return tile.name;
            }
        }

        return "";
    }

    public Vector3 GetCellCenterWorld(Vector3Int position)
    {
        return interactableMap.GetCellCenterWorld(position);
    }

}
