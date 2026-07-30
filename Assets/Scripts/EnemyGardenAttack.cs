using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyGardenAttack : MonoBehaviour
{
    private enum EnemyState
    {
        GoingToCrop,
        AttackingCrop,
        ChasingPlayer,
        Wandering
    }

    [Header("Crop Attack")]
    [SerializeField, Min(0.1f)]
    private float attackDuration = 2.5f;

    [Header("Wandering")]
    [SerializeField, Min(0.1f)]
    private float wanderRadius = 2f;

    private EnemyMovement movement;
    private TileManager tileManager;
    private Transform wanderCenter;

    private Transform playerTransform;
    private PlayerHealth playerHealth;

    private Vector3Int targetCropPosition;
    private bool hasCropTarget;

    private EnemyState state;
    private float attackFinishTime;
    private bool initialized;

    private Animator animator;

    public bool IsAttackingCrop => state == EnemyState.AttackingCrop;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();
    }

    public void Initialize(TileManager manager, Transform newWanderCenter)
    {
        tileManager = manager;
        wanderCenter = newWanderCenter;

        FindPlayerTwo();

        initialized = true;

        if (!TryTargetCrop())
        {
            if (PlayerIsAwake())
            {
                ChasePlayer();
            }
            else
            {
                StartWandering();
            }
        }
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (playerHealth == null)
        {
            FindPlayerTwo();
        }

        switch (state)
        {
            case EnemyState.GoingToCrop:

                if (movement.HasReachedTarget)
                {
                    BeginCropAttack();
                }

                break;

            case EnemyState.AttackingCrop:

                if (Time.time >= attackFinishTime)
                {
                    FinishCropAttack();
                }

                break;

            case EnemyState.ChasingPlayer:

                if (playerHealth != null &&
                    playerHealth.IsPassedOut)
                {
                    if (!TryTargetCrop())
                    {
                        StartWandering();
                    }
                }

                break;

            case EnemyState.Wandering:

                if (PlayerIsAwake())
                {
                    ChasePlayer();
                }
                else if (movement.HasReachedTarget)
                {
                    if (!TryTargetCrop())
                    {
                        ChooseWanderDestination();
                    }
                }

                break;
        }
    }

    private bool TryTargetCrop()
    {
        if (tileManager == null ||
            !tileManager.TryGetRandomDamageableCrop(
                out targetCropPosition))
        {
            hasCropTarget = false;
            return false;
        }

        hasCropTarget = true;
        state = EnemyState.GoingToCrop;

        Vector3 cropWorldPosition =
            tileManager.GetCellCenterWorld(
                targetCropPosition
            );

        movement.SetDestination(cropWorldPosition);

        return true;
    }

    private void BeginCropAttack()
    {
        state = EnemyState.AttackingCrop;

        attackFinishTime =
            Time.time + attackDuration;

        if(animator != null)
        {
            animator.SetBool("IsAttacking", true);
        }

        Debug.Log(
            $"Slime started attacking crop at " +
            $"{targetCropPosition}."
        );
    }

    private void FinishCropAttack()
    {

        if(animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
        if (hasCropTarget && tileManager != null)
        {
            tileManager.DamageCrop(
                targetCropPosition
            );
        }

        hasCropTarget = false;

        if (PlayerIsAwake())
        {
            ChasePlayer();
        }
        else if (!TryTargetCrop())
        {
            StartWandering();
        }
    }

    private void ChasePlayer()
    {
        if (playerTransform == null)
        {
            StartWandering();
            return;
        }

        state = EnemyState.ChasingPlayer;
        movement.SetTarget(playerTransform);
    }

    private void StartWandering()
    {
        state = EnemyState.Wandering;
        ChooseWanderDestination();
    }

    private void ChooseWanderDestination()
    {
        if (wanderCenter == null)
        {
            return;
        }

        Vector2 destination =
            (Vector2)wanderCenter.position +
            Random.insideUnitCircle * wanderRadius;

        movement.SetDestination(destination);
    }

    private bool PlayerIsAwake()
    {
        return playerHealth != null &&
               !playerHealth.IsPassedOut;
    }

    private void FindPlayerTwo()
    {
        Player[] players =
            FindObjectsByType<Player>(
                FindObjectsSortMode.None
            );

        foreach (Player player in players)
        {
            if (player.toolbarUI != null &&
                player.toolbarUI.inputType ==
                Toolbar_UI.InputType.Gamepad)
            {
                playerTransform = player.transform;
                playerHealth =
                    player.GetComponent<PlayerHealth>();

                return;
            }
        }
    }
}