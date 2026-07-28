using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 1.25f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.45f;

    private Rigidbody2D rb;
    private Transform target;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FindPlayerTwo();
    }

    private void FixedUpdate()
    {
        if( target == null)
        {
            FindPlayerTwo();
            SetMoving(false);
            return;
        }

        Vector2 offset = (Vector2)target.position - rb.position;

        if(offset.sqrMagnitude <= stoppingDistance * stoppingDistance)
        {
            SetMoving(false);
            return;
        }

        Vector2 direction = offset.normalized;

        Vector2 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;

        SetMoving(true);

        if(spriteRenderer != null &&
            Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }

        rb.MovePosition(nextPosition);
    }

    private void SetMoving(bool moving)

        {
        if(animator != null)
        {
            animator.SetBool("IsMoving", moving);
        }

   }
    private void FindPlayerTwo()
    {
        Player[] players =
            FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players)
        {
            if (player.toolbarUI != null &&
                player.toolbarUI.inputType ==
                Toolbar_UI.InputType.Gamepad)
            {
                target = player.transform;

                return;
            }
        }
    }
}
