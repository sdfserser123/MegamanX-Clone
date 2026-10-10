using UnityEngine;
using System.Collections;

public class SpikeWheelEnemy : Enemy 
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;

    private bool isWalled;
    private bool isTurning = false;
    private float direction;
    private float originalMoveSpeed;

    void Awake()
    {
        base.Start();
        rb.gravityScale = 7f;
        originalMoveSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeState && !isDead && !isTurning)
        {
            WallCollide();

            if (isWalled)
            {
                isTurning = true;
                StartCoroutine(TurningRoutine());
            }
        }
    }

    private void FixedUpdate()
    {
        if (activeState && !isDead && !isTurning)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }
    private void WallCollide()
    {
        isWalled = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(physicsCollider.bounds.center, physicsCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private IEnumerator TurningRoutine()
    {
        rb.linearVelocity = new Vector2(0f, 20f);

        yield return new WaitForSeconds(0.05f);

        while (!isGrounded())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            yield return null;
        }

        direction = -direction;
        moveSpeed = originalMoveSpeed;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * direction;
        transform.localScale = localScale;

        isTurning = false;
    }

    protected override void OnBecameVisible()
    {
        base.OnBecameVisible();

        physicsCollider.enabled = true;
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null)
        {
            direction = Mathf.Sign(player.position.x - transform.position.x);

            Vector3 localScale = transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * direction;
            transform.localScale = localScale;
        }
    }

    public override void GetHit(float damage)
    {
        if (isDead) return;
        if (currHp == 1 && damage == 1)
        {
            Animator animator = GetComponent<Animator>();
            isDead = true;
            Physics2D.IgnoreCollision(this.GetComponent<Collider2D>(), player.GetComponent<BoxCollider2D>(), true);
            animator.SetTrigger("Fell");
        }
        else
        {
            currHp -= damage;
            if (currHp <= 0)
            {
                Die();
            }
        }
    }

    protected override void OnBecameInvisible()
    {
        Physics2D.IgnoreCollision(physicsCollider, player.GetComponent<BoxCollider2D>(), false);
        Physics2D.IgnoreCollision(physicsCollider, player.GetComponent<BoxCollider2D>(), false);
        physicsCollider.enabled = false;
        base.OnBecameInvisible();
    }

    protected override void Die()
    {
        Physics2D.IgnoreCollision(physicsCollider, player.GetComponent<BoxCollider2D>(), true);
        physicsCollider.enabled = false;
        base.Die();
    }

    protected void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }

    protected override Vector2 getSize()
    {
        print("SpikeWheelEnemy size: " + new Vector2(physicsCollider.bounds.size.x, 4f));
        return new Vector2(0.1f, 0.1f);

    }
}
