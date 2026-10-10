using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Data Configuration")]
    [SerializeField] private ProjectileData data;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private Enemy attacker;
    [SerializeField] private LayerMask whatIsTarget;

    private bool hit;
    private float direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (data != null)
        {
            SetupItem(data);
        }
    }

    public void SetupItem(ProjectileData newData)
    {
        data = newData;

        if (spriteRenderer != null && data.projectileSprite != null)
        {
            spriteRenderer.sprite = data.projectileSprite;

        }

        if (animator != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((whatIsTarget.value & (1 << collision.gameObject.layer)) > 0)
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(data.damage);
                Deactivate();
            }
        }
    }

    public void SetDirection(float _direction, Enemy _attacker)
    {
        attacker = _attacker;
        direction = _direction;

        gameObject.SetActive(true);
        hit = false;
        //boxCollider.enabled = true;

        if(data.projectileType == ProjectileType.Bullet)
        {
            float localScaleX = transform.localScale.x;
            if (Mathf.Sign(localScaleX) != _direction)
            {
                localScaleX = -localScaleX;
            }

            transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (hit)
        {
            return;
        }

        float movementSpeed = data.speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnBecameInvisible()
    {
        Deactivate();
    }
}
