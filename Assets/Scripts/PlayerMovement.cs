using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Inventory_UI inventoryUI;
    public Storage_UI storageUI;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private bool movementLocked;

    public bool IsMovementLocked => movementLocked;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnMove(InputValue value)
    {
        if (movementLocked)
        {
            return;
        }

        moveInput = value.Get<Vector2>();
       

        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            Vector2 animDirection = moveInput.normalized;

            float x = 0f;
            float y = 0f;

            //snap to 8ish directions
            if (animDirection.x > 0.5f) x = 1f;
            else if (animDirection.x < -0.5f) x = -1f;

            if (animDirection.y > 0.5f) y = 1f;
            else if (animDirection.y < -0.5f) y = -1f;

            Vector2 snappedDir = new Vector2(x, y);

            //save last non zero direction for idle blend tree
            lastMoveDir = snappedDir;

            animator.SetFloat("MoveX", snappedDir.x);
            animator.SetFloat("MoveY", snappedDir.y);
            animator.SetFloat("LastMoveX", lastMoveDir.x);
            animator.SetFloat("LastMoveY", lastMoveDir.y);
                       
        }        
    }

    public Vector2 GetFacingDirection()
    {
        return lastMoveDir;
    }

    private void FixedUpdate()
    {
        if (movementLocked || inventoryUI.IsOpen)
        {
            return;
        }

        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }
}
