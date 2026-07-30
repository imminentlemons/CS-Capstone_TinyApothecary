using System.Collections;
using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField, Min(1)]
    private int contactDamage = 1;

    [Header("Attack Timing")]
    [SerializeField, Min(0f)]
    private float attackWindup = 0.5f;

    [SerializeField, Min(0f)]
    private float attackRecovery = 0.25f;

    [SerializeField, Min(0f)]
    private float attackCooldown = 0.75f;

    private Animator animator;
    private EnemyGardenAttack gardenAttack;

    private PlayerHealth playerInRange;
    private Coroutine attackRoutine;
    private float nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        gardenAttack =
            GetComponent<EnemyGardenAttack>();
    }

    private void Update()
    {
        TryStartAttack();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RegisterPlayer(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        RegisterPlayer(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        RemovePlayer(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RegisterPlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        RegisterPlayer(collision.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        RemovePlayer(collision.gameObject);
    }

    private void RegisterPlayer(GameObject other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            playerInRange = health;
        }
    }

    private void RemovePlayer(GameObject other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null &&
            health == playerInRange)
        {
            playerInRange = null;
        }
    }

    private void TryStartAttack()
    {
        if (playerInRange == null ||
            playerInRange.IsPassedOut ||
            attackRoutine != null ||
            Time.time < nextAttackTime ||
            (gardenAttack != null &&
             gardenAttack.IsAttackingCrop))
        {
            return;
        }

        attackRoutine =
            StartCoroutine(AttackPlayerRoutine());
    }

    private IEnumerator AttackPlayerRoutine()
    {
        PlayerHealth target = playerInRange;

        SetAttacking(true);

        // Animation wind-up before the hit lands.
        yield return new WaitForSeconds(attackWindup);

        bool targetStillValid =
            target != null &&
            target == playerInRange &&
            !target.IsPassedOut &&
            (gardenAttack == null ||
             !gardenAttack.IsAttackingCrop);

        if (targetStillValid)
        {
            target.TakeDamage(contactDamage);
        }

        yield return new WaitForSeconds(attackRecovery);

        SetAttacking(false);

        nextAttackTime =
            Time.time + attackCooldown;

        attackRoutine = null;
    }

    private void SetAttacking(bool attacking)
    {
        if (animator != null)
        {
            animator.SetBool(
                "IsAttacking",
                attacking
            );
        }
    }

    private void OnDisable()
    {
        SetAttacking(false);
    }
}