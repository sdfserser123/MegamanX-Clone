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

    private Rigidbody2D rb;
    private CircleCollider2D boxCollider;
    void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<CircleCollider2D>();
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
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
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

        rb.bodyType = RigidbodyType2D.Dynamic;
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
        rb.bodyType = RigidbodyType2D.Static;
        base.OnBecameInvisible();
    }

    protected override void Die()
    {
        rb.bodyType = RigidbodyType2D.Static;
        base.Die();
    }

    protected  void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }
}
