using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 1.25f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.45f;

    private Rigidbody2D rb;    
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Transform target;
    private Vector2 destination;
    private bool hasDestination;

    public bool HasReachedTarget { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hasDestination = false;
        HasReachedTarget = false;
    }

    public void SetDestination(Vector2 newDestination)
    {
        target = null;
        destination = newDestination;
        hasDestination = true;
        HasReachedTarget = false;
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition;

        if (target != null)
        {
            targetPosition = target.position;
        }
        else if(hasDestination)
        {
            targetPosition = destination;
        }
        else
        {
            SetMoving(false);
            return;
        }

        Vector2 offset = targetPosition - rb.position;

        if (offset.sqrMagnitude <=
            stoppingDistance * stoppingDistance)
        {
            HasReachedTarget = true;
            SetMoving(false);
            return;
        }

        HasReachedTarget = false;

        Vector2 direction = offset.normalized;

        Vector2 nextPosition =
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime;

        SetMoving(true);

        if (spriteRenderer != null &&
            Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }

        rb.MovePosition(nextPosition);
    }

    private void SetMoving(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", moving);
        }
    }
}