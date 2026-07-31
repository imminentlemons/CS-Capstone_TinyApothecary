using UnityEngine;

[RequireComponent(typeof(Item))]
public class Collectable : MonoBehaviour
{   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        
        if(player)
        {
            Item item = GetComponent<Item>();

            if (item != null)
            {
                if(player.inventoryManager.AddToToolbarThenBackpack(item))
                {
                    player.toolbarUI.Refresh();
                    player.inventoryUI.Refresh();
                    Destroy(gameObject);
                }
                else
                {
                    NotificationPopup_UI.Show("Backpack is full.");
                }
            }            
        }
    }
}
