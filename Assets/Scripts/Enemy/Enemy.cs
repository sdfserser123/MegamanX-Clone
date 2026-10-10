using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected float maxHp = 2f;
    [SerializeField] protected float collideDamage = 2f;
    [SerializeField] protected GameObject explodeEffectPrefab;
    [SerializeField] Transform[] explodePoint;
    [SerializeField] protected float damageInterval = 0.5f;
    [SerializeField] protected Transform player;
    [SerializeField] protected Transform attackPoint;

    MegamanXMovement playerMovement;

    private float damageTimer;
    protected Vector3 spawnPosition;
    protected float currHp;
    protected bool activeState = false;
    protected bool canSpawn = true;
    protected bool isLooking;
    public bool isDead = false;

    protected Renderer enemyRenderer;
    protected SpriteRenderer sr;
    protected Collider2D physicsCollider;
    protected CapsuleCollider2D hitBoxCollider;
    protected Rigidbody2D rb;
    protected Camera mainCamera;
    protected GameObject playerObj;
    protected GameObject attackObj;

    public float CurrHp => currHp;

    protected virtual void Start()
    {
        currHp = maxHp;
        spawnPosition = transform.position;

        mainCamera = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        enemyRenderer = GetComponent<Renderer>();
        physicsCollider = GetComponent<BoxCollider2D>();
        if (physicsCollider == null) physicsCollider = GetComponent<CircleCollider2D>();
        hitBoxCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        playerObj = GameObject.FindWithTag("Player");
        attackObj = GameObject.FindWithTag("Player Attack");

        
        Physics2D.IgnoreCollision(physicsCollider, playerObj.GetComponent<Collider2D>(), true);

        if (hitBoxCollider != null && attackObj != null)
        {
            Collider2D attackCollider = attackObj.GetComponent<Collider2D>();
            if (attackCollider != null)
            {
                Physics2D.IgnoreCollision(hitBoxCollider, attackCollider, true);
            }
        }
        UpdateEnemyActivation(activeState);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            print("Player collided with enemy");
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(collideDamage);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.CompareTag("Player"))
        {
            print("Player collided with enemy");
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(collideDamage);
            }
        }
    }


    public virtual void GetHit(float damage)
    {
        if (isDead) return;
        currHp -= damage;
        if (currHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.bodyType = RigidbodyType2D.Static;
        activeState = false;
        HandleExplosion();
        if (sr != null) sr.enabled = false;
        UpdateEnemyActivation(false);
        transform.position = spawnPosition;
        currHp = maxHp;
        StopAllCoroutines();
        StartCoroutine(WaitToRespawn());
    }

    protected virtual Vector2 getSize()
    {
        Vector2 size = physicsCollider.bounds.size;
        return size;
    }
    private IEnumerator WaitToRespawn()
    {
        while (true)
        {
            Vector3 viewPos = mainCamera.WorldToViewportPoint(spawnPosition);
            Vector3 curPos = mainCamera.WorldToViewportPoint(transform.position);
            Vector2 size = getSize();
            float objectSizeX = size.x + 0.01f;
            float objectSizeY = size.y + 0.01f;
            bool isSpawnPointVisible = viewPos.x >= (0 - objectSizeX) && viewPos.x <= (1 + objectSizeX) &&
                                       viewPos.y >= (0 - objectSizeY) && viewPos.y <= (1 + objectSizeY) && viewPos.z > 0;
            bool isCurrentPosVisible = curPos.x >= (0 - objectSizeX) && curPos.x <= (1 + objectSizeX) &&
                                     curPos.y >= (0 - objectSizeY) && curPos.y <= (1 + objectSizeY) && curPos.z > 0;

            yield return new WaitForSeconds(0.2f );

            if (!isSpawnPointVisible && !isCurrentPosVisible)
            {
                transform.position = spawnPosition;
                currHp = maxHp;
                isDead = false;

                if (sr != null) sr.enabled = true;


                UpdateEnemyActivation(true);
                yield break;
            }
        }
    }

    protected virtual void OnBecameVisible()
    {
        if (!activeState && !isDead)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            activeState = true;
            currHp = maxHp;
            if (sr != null) sr.enabled = true;
            UpdateEnemyActivation(true);
        }
    }

    protected virtual void OnBecameInvisible()
    {
        if (!isDead)
        {
            rb.bodyType = RigidbodyType2D.Static;
            activeState = false;
            if (sr != null) sr.enabled = false;
            UpdateEnemyActivation(false);
            transform.position = spawnPosition;
            StopAllCoroutines();
            StartCoroutine(WaitToRespawn());
        }
    }


    private void HandleExplosion()
    {
        if (explodePoint != null && explodePoint.Length > 0)
        {
            foreach (Transform point in explodePoint)
            {
                if (point != null && explodeEffectPrefab != null)
                {
                    Instantiate(explodeEffectPrefab, point.position, Quaternion.identity);
                }
            }
        }
        else
        {
            if (explodeEffectPrefab != null)
            {
                Instantiate(explodeEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
    private void UpdateEnemyActivation(bool dynamicState)
    {
        if (physicsCollider != null) physicsCollider.enabled = dynamicState;
        if (hitBoxCollider != null) hitBoxCollider.enabled = dynamicState;
    }

}