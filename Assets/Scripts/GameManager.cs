using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public ItemManager itemManager;
    public TileManager tileManager;

    public Player player;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }        

        itemManager = GetComponent<ItemManager>();
        tileManager = GetComponent<TileManager>();

        player = FindFirstObjectByType<Player>();
    }
}
