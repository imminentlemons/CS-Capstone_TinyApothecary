using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private enum EnemyType
    {
        Slime,
        Bat
    }

    [Header("Health")]
    [SerializeField, Min(1)]
    private int maxHealth = 3;

    [Header("Hit Flash")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [Header("Death")]
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyType enemyType;

    [SerializeField]
    private Color hitFlashColor =
        new Color(1f, 1f, 1f, 0.35f);

    [SerializeField, Min(0.01f)]
    private float hitFlashDuration = 0.12f;

    private int currentHealth;
    private bool isDead;

    private EnemyMovement enemyMovement;
    private EnemyGardenAttack gardenAttack;
    private EnemyContactDamage contactDamage;
    private Collider2D[] enemyColliders;

    private Color normalColor;
    private Coroutine flashRoutine;

    public static event Action EnemyDefeated;


    private void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            normalColor = spriteRenderer.color;
        }

        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }

        enemyMovement = GetComponent<EnemyMovement>();
        gardenAttack = GetComponent<EnemyGardenAttack>();
        contactDamage = GetComponent<EnemyContactDamage>();

        enemyColliders = GetComponentsInChildren<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth =
            Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            $"{name} took {damage} damage. " +
            $"{currentHealth}/{maxHealth} HP remaining."
        );

        if (currentHealth == 0)
        {
            PlayDeathSound();
            Die();
        }
        else
        {
            AudioManager.PlayMonsterDamage();
            PlayHitFlash();
        }
    }

    private void PlayDeathSound()
    {
        if(enemyType == EnemyType.Bat)
        {
            AudioManager.PlayBatDeath();
        }
        else
        {
            AudioManager.PlaySlimeDeath();
        }
    }

    private void PlayHitFlash()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            spriteRenderer.color = normalColor;
        }

        flashRoutine =
            StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = hitFlashColor;

        yield return new WaitForSeconds(
            hitFlashDuration
        );

        spriteRenderer.color = normalColor;
        flashRoutine = null;
    }

    private void Die()
    {
        isDead = true;

        EnemyDefeated?.Invoke();

        Debug.Log($"{name} was defeated.");

        if(flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if(spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        if(enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        if(gardenAttack != null)
        {
            gardenAttack.enabled = false;
        }

        if(contactDamage != null)
        {
            contactDamage.enabled = false;
        }

        foreach(Collider2D enemyCollider in enemyColliders)
        {
            enemyCollider.enabled = false;
        }

        if(animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsAttacking", false);
            animator.SetTrigger("Die");
        }

        else
        {
            Destroy(gameObject);
        }
        
    }

    public void FinishDeath()
    {
        Destroy(gameObject);
    }
}
