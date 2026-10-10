using UnityEngine;
using System.Collections;

public class BunnyEnemy : Enemy
{
    Animator animator;


    [SerializeField] Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize;

    [SerializeField] float jumpForce = 10f;

    [SerializeField] GameObject[] Bullets;

    float direction;
    bool isProcessing = false;
    private bool wasGrounded = true;

    void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("VelocityY", rb.linearVelocity.y);
        if (activeState && !isDead)
        {
            bool grounded = isGrounded();

            if (grounded && !wasGrounded && !isProcessing)
            {
                animator.SetTrigger("Land");
                StartCoroutine(Processing());
            }

                wasGrounded = grounded;
        }
    }

    private void Jump()
    {
        if (isGrounded() && player != null)
        {
            rb.linearVelocity = new Vector2(10f * direction, jumpForce);
        }
    }

    private void Attack()
    {
        if (isGrounded() && player != null)
        {
            GameObject currentBullet = findBullet();
            if (currentBullet == null) return;

            currentBullet.transform.position = attackPoint.position;
            currentBullet.GetComponent<EnemyProjectile>().SetDirection(Mathf.Sign(transform.localScale.x), this);
        }

    }

    private GameObject findBullet()
    {
        GameObject[] targetPool = Bullets;

        for (int i = 0; i < targetPool.Length; i++)
        {
            if (!targetPool[i].activeInHierarchy)
            {
                return targetPool[i];
            }
        }
        return null;
    }

    private IEnumerator Processing()
    {
        isProcessing = true;

        yield return new WaitForSeconds(0.5f);

        if (activeState && !isDead)
        {
            if (player != null)
            {
                direction = Mathf.Sign(player.position.x - transform.position.x);
                if (direction != 0)
                {
                    transform.localScale = new Vector3(direction, 1f, 1f);
                }
            }

            yield return new WaitForSeconds(0.5f);

            int randomNumber = Random.Range(0, 2);

            switch (randomNumber)
            {
                case 0:
                    animator.ResetTrigger("Land");
                    animator.SetTrigger("Jump");
                    yield return new WaitForSeconds(0.2f);
                    break;

                case 1:
                    animator.SetTrigger("Attack");
                    yield return new WaitForSeconds(1.0f);
                    StartCoroutine(Processing());
                    break;
            }
        }

        isProcessing = false;
    }

    protected override void Die()
    {
        isProcessing = false;
        base.Die();
    }

    protected override void OnBecameInvisible()
    {
        isProcessing = false;
        base.OnBecameInvisible();
    }

    protected override void OnBecameVisible()
    {
        Processing();
        base.OnBecameVisible();
    }

    private bool isGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, LayerMask.GetMask("Ground"));
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}