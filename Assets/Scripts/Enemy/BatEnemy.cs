using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BatEnemy : Enemy
{
    [SerializeField] Transform ceilingCheck;
    [SerializeField] private Vector2 ceilingCheckSize;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float updateInterval = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    private bool attackable = false;
    private bool isActive = false;
    private bool isInWall = false;

    private Animator animator;
    private Coroutine trackingCoroutine;
    private Coroutine behaviorCycleCoroutine;
    private Vector2 direction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0f;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (activeState && player != null)
        {
            if (!isActive)
            {
                isActive = true;
            }
            if (behaviorCycleCoroutine == null)
            {
                behaviorCycleCoroutine = StartCoroutine(EnemyBehaviorCycleRoutine());
            }

            if (attackable)
            {
                if (trackingCoroutine == null)
                {
                    trackingCoroutine = StartCoroutine(TrackPlayerRoutine());
                }

                if (checkingCeiling())
                {
                    isInWall = true;
                }
                else
                {
                    isInWall = false;
                }
            }
            else
            {
                if (trackingCoroutine != null)
                {
                    StopCoroutine(trackingCoroutine);
                    trackingCoroutine = null;
                }

                if (!checkingCeiling() || isInWall)
                {
                    direction = Vector2.up;
                    rb.linearVelocity = direction * 10;
                }
                else
                {
                    isActive = false;
                    rb.linearVelocity = Vector2.zero;
                }
            }

            if (!checkingCeiling() && isInWall)
            {
                isInWall = false;
            }
        }
        else
        {
            ResetAllBehaviors();
        }
    }

    private void FixedUpdate()
    {
        animator.SetBool("active", isActive);
    }

    private IEnumerator EnemyBehaviorCycleRoutine()
    {
        while (true)
        {
            attackable = true;
            yield return new WaitForSeconds(10f);
            attackable = false;
            yield return new WaitForSeconds(2f);

        }
    }

    private IEnumerator TrackPlayerRoutine()
    {
        while (true)
        {
            if (player != null)
            {
                direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void ResetAllBehaviors()
    {
        if (behaviorCycleCoroutine != null)
        {
            StopCoroutine(behaviorCycleCoroutine);
            behaviorCycleCoroutine = null;
        }
        if (trackingCoroutine != null)
        {
            StopCoroutine(trackingCoroutine);
            trackingCoroutine = null;
        }

        attackable = false;
        rb.linearVelocity = Vector2.zero;
    }

    protected void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private bool checkingCeiling()
    {
        return Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer) != null;
    }

    protected void OnDrawGizmosSelected()
    {
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(ceilingCheck.position, ceilingCheckSize);
        }
    }
}