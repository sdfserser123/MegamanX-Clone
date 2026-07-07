using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHp;
    private MegamanXMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool dead = false;
    public float currentHp { get; private set; }

    private bool isRecovering = false;
    void Awake()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<MegamanXMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (currentHp <= 5)
        {
            playerMovement.isLowHp = true;
        }
        else
        {
            playerMovement.isLowHp = false;
        }
    }

    public void TakeDamage(float _damage)
    {
        if (dead || isRecovering) return;

        currentHp = Mathf.Clamp(currentHp - _damage, 0, maxHp);

        if (currentHp > 0)
        {
            if (playerMovement != null)
            {
                playerMovement.TriggerHurt();
            }
            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            if (!dead)
            {
                dead = true;

                animator.SetBool("run", false);
                animator.SetBool("jump", false);
                animator.SetBool("drop", false);
                animator.SetBool("grounded", true);

                animator.SetTrigger("die");
                var rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.linearVelocity = Vector2.zero;
                }

                if (playerMovement != null)
                {
                    playerMovement.enabled = false;
                    playerMovement.isDead = true;
                }
            }
        }
    }

    public void Heal(float healingAmount)
    {
        if (dead) return;

        StartCoroutine(FreezeAndHealRoutine(healingAmount));
    }

    private IEnumerator FreezeAndHealRoutine(float healingAmount)
    {
        Time.timeScale = 0f;

        float targetHp = Mathf.Min(currentHp + healingAmount, maxHp);


        while (currentHp < targetHp)
        {
            currentHp += 1f;
            currentHp = Mathf.Min(currentHp, targetHp); 

            yield return new WaitForSecondsRealtime(0.07f);
        }

        currentHp = targetHp;

        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 1f;
    }

    private void TriggerInvincibleEffect()
    {
        StartCoroutine(FadingRoutine());
    }
    private IEnumerator InvincibilityRoutine()
    {
        isRecovering = true;
        TriggerInvincibleEffect();

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        yield return new WaitForSeconds(1.5f);

        isRecovering = false;

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

    private IEnumerator FadingRoutine()
    {
        for (int i = 0; i < 25; i++)
        {
            Color c = spriteRenderer.color;
            c.a = 0.2f;
            spriteRenderer.color = c;

            yield return new WaitForSeconds(0.02f);

            c.a = 1.0f;
            spriteRenderer.color = c;

            yield return new WaitForSeconds(0.02f);
        }
    }
}

    
