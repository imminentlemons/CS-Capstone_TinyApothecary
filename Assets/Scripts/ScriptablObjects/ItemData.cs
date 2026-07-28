using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Item Data", menuName = "Item Data", order = 50)]
public class ItemData : ScriptableObject
{
    public string itemName = "Item Name";

    [TextArea(2, 4)]
    public string flavorText;

    public Sprite icon;

    public ItemType itemType;

    public ItemData cropToGrow;
    public ToolType toolType;

    [Header("Crop Growth Sprites")]
    public Tile wateredSeedTile;
    public Tile seedlingTile;
    public Tile sproutTile;
    public Tile matureTile;
    public Tile adultTile;

    [Header("Economy")]
    [Tooltip("Seed purchase cost or potion sale reward.")]
    [Min(0)] public int price;

    public enum ItemType
    {
        Ingredient,
        Seed,
        Potion,
        Tool
    }

    public enum ToolType
    {
        None,
        Hoe,
        WateringCan
    }
}