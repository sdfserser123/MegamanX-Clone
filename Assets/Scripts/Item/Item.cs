using UnityEngine;

public abstract class Item : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected BoxCollider2D physicsCollider;
    protected BoxCollider2D triggerCollider;
    protected Animator animator;

    private static int globalSortingOrder = 0;

    [Header("Physics Settings (Item Parent)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float bounceForce = 5f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        if (colliders.Length >= 2)
        {
            foreach (var col in colliders)
            {
                if (col.isTrigger) triggerCollider = col;
                else physicsCollider = col;
            }

            if (triggerCollider != null) triggerCollider.isTrigger = true;
            if (physicsCollider != null) physicsCollider.isTrigger = false;
        }
    }

    protected virtual void Start()
    {
        if (rb != null)
        {
            float randomXVelocity = Random.Range(-3f, 3f);

            rb.linearVelocity = new Vector2(randomXVelocity, bounceForce);
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            globalSortingOrder++;
            sr.sortingOrder = globalSortingOrder;
            if (globalSortingOrder > 5000) globalSortingOrder = 0;
        }
    }

    protected virtual void Update()
    {
        if (IsGrounded())
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetTrigger("onGround");
            }
        }
    }

    private bool IsGrounded()
    {
        if (physicsCollider == null) return false;

        RaycastHit2D raycastHit = Physics2D.BoxCast(
            physicsCollider.bounds.center,
            physicsCollider.bounds.size,
            0f,
            Vector2.down,
            -0.1f,
            groundLayer
        );

        return raycastHit.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyEffect(collision.gameObject);
            Destroy(gameObject);
        }
    }

    protected abstract void ApplyEffect(GameObject player);
}