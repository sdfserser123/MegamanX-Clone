using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MegamanXMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 7f;
    public float dashSpeed = 14f;
    public float    jumpForce = 15f;
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(12f, 15f);

    [Header("Timers & Cooldowns")]
    public float dashDuration = 0.3f;
    public float wallJumpDuration = 0.2f;
    [Tooltip("Timing for Wall Slide")]
    public float wallSlideCooldownDuration = 0.2f;

    [Header("Variable Jump Height")]
    [Range(0f, 1f)]
    [Tooltip("")]
    public float jumpCutMultiplier = 0.4f;

    [Header("Collision Checks")]
    private Collider2D[] wallColliders = new Collider2D[1];
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ceilingCheck;
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] public Vector2 wallCheckSize = new Vector2(2.2f, 1f);
    public Vector2 ceilingCheckSize = new Vector2(0.5f, 0.1f);

    [Header("Wall Push Off")]
    public Vector2 wallPushOffForce = new Vector2(5f, 2f);
    public float wallPushOffDuration = 0.1f;
    private bool isPushingOffWall = false;

    [Header("X Buster")]
    public float pointSize;
    public Transform busterPoint;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    [SerializeField] private PhysicsMaterial2D slipperyMaterial;
    [SerializeField] private PhysicsMaterial2D slipperyMaterial2;
    private float moveInput;
    private float wallSlideCooldown;
    private float dashJumpTimer = 0f;
    public bool movable = true;
    private bool isGrounded;
    private bool isWalled;
    private bool isCeiling;
    private bool isDashing;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool isDashJumping;
    private bool isHoldingJumpKey;
    private bool canCancleJump = true;
    private bool performingWallJump;
    private float velocityY;
    public bool isMoving = false;
    public bool isAttacking = false;
    public float targetAttackingWeight = 0f;
    private bool isHurt = false;
    public bool isDead = false;
    public bool isLowHp = false;
    private float statusWeight = 0f;

    [SerializeField] float DefaultGravityScale;

    private int facingDirection = 1;

    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        wallSlideCooldown = wallSlideCooldownDuration;
    }

    void Update()
    {
        if (isHurt) return;

        velocityY = rb.linearVelocity.y;

        isMoving = rb.linearVelocity != new Vector2 (0f,0f);

        Inputs();
        CheckCollisions();
        HandleFlip();
        HandleWallSlide();

        if (isAttacking)
        {
            targetAttackingWeight = 1;
        }
        else
        {
            targetAttackingWeight = 0;
        }

        if (isLowHp)
        {
            statusWeight = 1;
        }
        else
        {
            statusWeight = 0;
        }

            UpdateAnimatorParameters();

        wallSlideCooldown += Time.deltaTime;
        dashJumpTimer += Time.deltaTime;

        if ((isGrounded || isWalled || isWallSliding) && isDashJumping && dashJumpTimer > 0.4f)
        {
            isDashing = false;
            isDashJumping = false;
        }

     
    }

    void FixedUpdate()
    {
        if (performingWallJump || isHurt) return;

        HandleMove();

        FixedUpdateAnimatorParameters();
    }

    private void Inputs()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = 0f;
        if (movable)
        {
            if (keyboard.rightArrowKey.isPressed) moveInput = 1f;
            if (keyboard.leftArrowKey.isPressed) moveInput = -1f;
        }

        isHoldingJumpKey = keyboard.xKey.isPressed;

        if (isWallJumping) return;
        if (keyboard.xKey.wasPressedThisFrame)
        {
            if (isDashing)
            {
                dashJumpTimer = 0f;
                isDashJumping = true;
                isDashing = false;
                StopAllCoroutines();
            }

            if ((isWallSliding || isWalled) && !isGrounded)
            {
                isWallJumping = true;
                movable = false;
                StartCoroutine(UnifiedWallJumpRoutine());
            }
            else if (isGrounded)
            {
                wallSlideCooldown = 0f;
                Jump();
            }
        }
        if (keyboard.xKey.wasReleasedThisFrame && canCancleJump)
        {
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
        }

        if (keyboard.zKey.wasPressedThisFrame && isGrounded && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void HandleMove()
    {
        if (performingWallJump || isPushingOffWall) return;

        if (isGrounded)
        {
            rb.sharedMaterial = null;
            boxCollider.sharedMaterial = slipperyMaterial2;
        }
        else
        {
            
            boxCollider.sharedMaterial = null;
            rb.sharedMaterial = slipperyMaterial;
        }
        if (isCeiling && !isGrounded && !isWallSliding && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * walkSpeed, 0f);
            movable = true;
        }

        if (isWallJumping)
        {
            rb.gravityScale = 0f;
            return;
        }

        if (isWallSliding)
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            return;
        }

        if (isDashJumping)
        {
            rb.gravityScale = DefaultGravityScale;
            rb.linearVelocity = new Vector2(moveInput * dashSpeed, rb.linearVelocity.y);
            return;
        }

        if (isDashing)
        {
            rb.gravityScale = DefaultGravityScale;
            rb.linearVelocity = new Vector2(facingDirection * dashSpeed, rb.linearVelocity.y);
            return;
        }

        rb.gravityScale = DefaultGravityScale;
        rb.linearVelocity = new Vector2(moveInput * walkSpeed, rb.linearVelocity.y);
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetBool("jump", !isGrounded && !isWallSliding && (rb.linearVelocity.y > 0f));
        animator.SetBool("drop", !isGrounded && !isWallSliding && (rb.linearVelocity.y < 0f));
        animator.SetFloat("VelocityY", rb.linearVelocity.y);
        animator.SetFloat("AttackWeight", targetAttackingWeight);
        animator.SetFloat("status", statusWeight);
    }

    private void FixedUpdateAnimatorParameters()
    {
        animator.SetBool("grounded", isGrounded);
        animator.SetBool("run", moveInput != 0 && isGrounded && !isDashing);
        animator.SetBool("wallSlide", isWallSliding);
    }
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private IEnumerator WallJumpRoutine()
    {

        performingWallJump = true;
        isWallSliding = false;

        animator.SetTrigger("wallJump");

        yield return new WaitForSeconds(0.07f);

        rb.gravityScale = DefaultGravityScale;
        rb.linearVelocity = new Vector2(facingDirection * wallJumpForce.x, wallJumpForce.y);

        yield return new WaitForSeconds(0.03f);
        Flip();
        StopWallJump();

        yield return new WaitForSeconds(wallJumpDuration);
        movable = true;
        performingWallJump = false;
        canCancleJump = true;
        if (!isHoldingJumpKey)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

    }

    public void TriggerHurt()
    {
        if (isHurt) return;
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        movable = false;

        rb.linearVelocity = new Vector2(4 * -facingDirection, 6f);

        rb.sharedMaterial = null;

        animator.SetBool("run", false);
        animator.SetBool("jump", false);
        animator.SetBool("drop", false);

        animator.SetTrigger("hurt");

        yield return new WaitForSeconds(0.5f);

        isHurt = false;
        movable = true;
    }

    private IEnumerator UnifiedWallJumpRoutine()
    {
        performingWallJump = true;
        isWallSliding = false;

        int wallDir = GetWallDirection();
        if (wallDir == 0) wallDir = facingDirection;

        facingDirection = wallDir;
        transform.localScale = new Vector3(facingDirection, 1, 1);
        Flip();
        animator.SetTrigger("wallJump");

        yield return new WaitForSeconds(0.07f);
        Flip();
        facingDirection = -wallDir;
        transform.localScale = new Vector3(facingDirection, 1, 1);

        rb.gravityScale = DefaultGravityScale;
        rb.linearVelocity = new Vector2(facingDirection * wallJumpForce.x, wallJumpForce.y);

        yield return new WaitForSeconds(0.03f);
        Flip();
        StopWallJump();

        yield return new WaitForSeconds(wallJumpDuration);

        movable = true;
        performingWallJump = false;
        canCancleJump = true;

        if (!isHoldingJumpKey)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private void StopWallJump()
    {
        canCancleJump = false;
        isWallJumping = false;
        animator.SetBool("wallJump", false);
    }

    private IEnumerator DashRoutine()
    {
        print("dash");
        isDashing = true;
        if (moveInput != 0)
        {
            facingDirection = (int)moveInput;
            transform.localScale = new Vector3(facingDirection, 1, 1);
        }

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private void HandleWallSlide()
    {
        if (isWallJumping || isPushingOffWall)
        {
            isWallSliding = false;
            return;
        }

        int wallDir = GetWallDirection();

        if (!isGrounded && isWalled && rb.linearVelocity.y < 0f)
        {
            bool wallOnRight = wallDir == 1;
            bool wallOnLeft = wallDir == -1;

            if (moveInput == 0f)
            {
                isWallSliding = false;
            }

            if (moveInput != 0f && ((wallOnRight && moveInput > 0f) || (wallOnLeft && moveInput < 0f)))
            {
                isWallSliding = true;

                if (facingDirection != wallDir)
                {
                    facingDirection = wallDir;
                    transform.localScale = new Vector3(facingDirection, 1, 1);
                }
                Flip();
            }

            if (isWallSliding && ((wallOnRight && moveInput < 0f) || (wallOnLeft && moveInput > 0f)))
            {
                isWallSliding = false;
                StartCoroutine(WallPushOffCoroutine());
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    private IEnumerator WallPushOffCoroutine()
    {
        isPushingOffWall = true;
        movable = false;
        isWallSliding = false;

        facingDirection = -facingDirection;
        transform.localScale = new Vector3(facingDirection, 1, 1);

        rb.gravityScale = DefaultGravityScale;
        rb.linearVelocity = new Vector2(facingDirection * wallPushOffForce.x, wallPushOffForce.y);

        yield return new WaitForSeconds(wallPushOffDuration);

        movable = true;
        isPushingOffWall = false;
    }

    public bool canAttack()
    {
        return !isHurt && !isDead;
    }

    private void HandleFlip()
    {
        if (isDashing || isWallJumping || isWallSliding) return;

        if (moveInput > 0 && facingDirection == -1)
        {
            Flip();
        }
        else if (moveInput < 0 && facingDirection == 1)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingDirection = -facingDirection;
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    private void CheckCollisions()
    {
        wallColliders[0] = null;

        int wallCount = Physics2D.OverlapBoxNonAlloc(wallCheck.position, wallCheckSize, 0f, wallColliders, groundLayer);
        isWalled = wallCount > 0 && wallColliders[0] != null;

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        isCeiling = Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer);
    }

    private int GetWallDirection()
    {
        if (!isWalled || wallColliders[0] == null) return 0;

        Vector3 closestPoint = wallColliders[0].ClosestPoint(transform.position);
        return (closestPoint.x - transform.position.x) > 0 ? 1 : -1;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(ceilingCheck.position, ceilingCheckSize);
        }
        if (busterPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(busterPoint.position, pointSize);
        }
    }
}