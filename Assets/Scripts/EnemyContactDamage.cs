using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int contactDamage = 1;

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other.gameObject);
    }

    private void TryDamage(GameObject other)
    {
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if(playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }
}
