using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField, Min(0f)] private float damageCooldown = 1.25f;

    [Header("Health Regeneration")]
    [SerializeField, Min(0f)] private float regenerationDelay = 5f;
    [SerializeField, Min(0.1f)] private float regenerationInterval = 3f;

    [Header("Pass Out")]
    [SerializeField, Min(0f)] private float passedOutSeconds = 3f;
    [SerializeField, Min(0f)] private float recoveryProtection = 2f;

    [Header("Passed Out Appearance")]
    [SerializeField] private Sprite passedOutSprite;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;

    [Header("UI")]
    [SerializeField] private Health_UI healthUI;

    private int currentHealth;
    private float nextDamageTime;
    private float nextRegenerationTime;
    private bool isPassedOut;

    private Player playerActions;
    private PlayerMovement movement;

    public bool IsPassedOut => isPassedOut;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerActions = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        healthUI.SetHealth(currentHealth);
    }

    private void Update()
    {
        if (isPassedOut ||
            currentHealth >= maxHealth ||
            Time.time < nextRegenerationTime)
        {
            return;
        }

        currentHealth++;
        healthUI.SetHealth(currentHealth);

        nextRegenerationTime =
            Time.time + regenerationInterval;

        Debug.Log(
            $"P2 regenerated 1 HP. " +
            $"{currentHealth}/{maxHealth} HP."
        );
    }

    public void TakeDamage(int damage)
    {
        if(isPassedOut || damage <= 0 || Time.time < nextDamageTime)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        nextDamageTime = Time.time + damageCooldown;
        nextRegenerationTime = Time.time + regenerationDelay;

        healthUI.SetHealth(currentHealth);

        Debug.Log($"P2 took {damage} damage." +
            $"{currentHealth}/{maxHealth} HP remaining.");

        if(currentHealth == 0)
        {
            StartCoroutine(PassOutRoutine());
        }
    }

    private IEnumerator PassOutRoutine()
    {
        isPassedOut = true;

        if(movement != null)
        {
            movement.SetMovementLocked(true);
        }

        if(playerActions != null)
        {
            playerActions.enabled = false;
        }

        if(playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        if(playerSpriteRenderer != null && passedOutSprite != null)
        {
            playerSpriteRenderer.sprite = passedOutSprite;
        }

        Debug.Log($"P2 passed out for {passedOutSeconds} seconds.");

        yield return new WaitForSeconds(passedOutSeconds);

        currentHealth = maxHealth;
        healthUI.SetHealth(currentHealth);

        if(playerAnimator != null)
        {
            playerAnimator.enabled = true;
        }

        if(playerActions != null)
        {
            playerActions.enabled = true;
        }

        if(movement != null)
        {
            movement.SetMovementLocked(false);
        }

        nextDamageTime = Time.time + recoveryProtection;

        isPassedOut = false;

        Debug.Log("P2 recovered.");
    }
}
