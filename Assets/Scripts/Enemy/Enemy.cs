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

    private float damageTimer;
    protected Vector3 spawnPosition;
    protected float currHp;
    protected bool activeState = false;
    protected bool canSpawn = true;
    protected bool isLooking;
    protected bool isDead = false;

    private Renderer enemyRenderer;
    private SpriteRenderer sr;
    private Collider2D enemyCollider;
    private Camera mainCamera;

    public float CurrHp => currHp;

    protected virtual void Start()
    {
        currHp = maxHp;
        spawnPosition = transform.position;

        mainCamera = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        enemyRenderer = GetComponent<Renderer>();
        enemyCollider = GetComponent<Collider2D>();

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
        activeState = false;
        HandleExplosion();
        if (sr != null) sr.enabled = false;
        UpdateEnemyActivation(false);

        StopAllCoroutines();
        StartCoroutine(WaitToRespawn());
    }
    private IEnumerator WaitToRespawn()
    {
        
        while (true)
        {
            Vector3 viewPos = mainCamera.WorldToViewportPoint(spawnPosition);
            Vector3 curPos = mainCamera.WorldToViewportPoint(transform.position);
            bool isSpawnPointVisible = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && curPos.z > 0;
            bool isCurrentPosVisible = curPos.x >= 0 && curPos.x <= 1 && curPos.y >= 0 && curPos.y <= 1 && curPos.z > 0;
            yield return new WaitForSeconds(0.5f);
            if (!isSpawnPointVisible && !isCurrentPosVisible)
            {
               
                transform.position = spawnPosition;
                currHp = maxHp;
                isDead = false;
                sr.enabled = true;
                yield break;
            }

            
        }
    }

    protected virtual void OnBecameVisible()
    {
        if (!activeState && !isDead)
        {
            activeState = true;
            if (sr != null) sr.enabled = true;
            UpdateEnemyActivation(true);
        }
    }

    protected virtual void OnBecameInvisible()
    {
        if (!isDead)
        {
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
        if (enemyCollider != null) enemyCollider.enabled = dynamicState;
    }
}