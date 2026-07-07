using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float attackCooldown = 0.1f;
    [SerializeField] Transform bulletPoint;

    [SerializeField] GameObject[] normalBullets;
    [SerializeField] GameObject[] charge1Bullets;
    [SerializeField] GameObject[] charge2Bullets;

    [SerializeField] private float charge1Time = 0.7f;
    [SerializeField] private float charge2Time = 1.8f;

    [SerializeField] private ChargingController chargingController;

    [SerializeField] GameObject[] bullets;
    private MegamanXMovement playerMovement;
    private SpriteBlink spriteBlink;
    private float bulletNumber = 3;
    private bool Attackable = true;
    private float attackTimer = 0f;

    private float chargeTimer = 0f;
    private bool isCharging = false;
    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<MegamanXMovement>();
        spriteBlink = GetComponent<SpriteBlink>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        attackTimer += Time.deltaTime;

        if (keyboard.cKey.wasPressedThisFrame && Attackable && bulletNumber > 0 && playerMovement.canAttack())
        {
            playerMovement.isAttacking = true;
            attackTimer = 0f;
            if (!playerMovement.isMoving)
            {
                print("attack");
                bulletNumber -= 1;
                anim.SetTrigger("normal attack");
                Attack(0);
            }
            else
            {
                attackTimer = 0f;
                bulletNumber -= 1;
                Attack(0);
            }
        }

        if (keyboard.cKey.isPressed && playerMovement.canAttack())
        {
            isCharging = true;
            chargeTimer += Time.deltaTime;

            if (chargeTimer >= charge2Time)
            {
                chargingController.chargeLevel = 1;
            }
            else if (chargeTimer >= charge1Time)
            {
                chargingController.isCharging = true;
                spriteBlink.isFlashing = true;
            }
        }

        if (keyboard.cKey.wasReleasedThisFrame && isCharging)
        {
            int chargeLevel = 0;

            if (chargeTimer >= charge2Time)
            {
                chargeLevel = 2;
            }
            else if (chargeTimer >= charge1Time) {
                chargeLevel = 1;
            }
            TryAttack(chargeLevel);

            spriteBlink.isFlashing = false;
            chargingController.chargeLevel = 0;
            chargingController.isCharging = false;
            isCharging = false;
            chargeTimer = 0f;
        }

        if (!keyboard.cKey.isPressed && isCharging)
        {
            isCharging = false;
            chargeTimer = 0f;
        }

        if (attackTimer > 0.3f)
        {
            finishAttack();
        }

    }

    private void TryAttack(int chargeLevel)
    {
        if (!Attackable || (bulletNumber <= 0)) return;

        playerMovement.isAttacking = true;
        attackTimer = 0f;

        if (!playerMovement.isMoving)
        {
            anim.SetTrigger("normal attack");
        }

            bulletNumber -= 1;

        Attack(chargeLevel);
    }

    private void Attack(int chargeLevel)
    {
        GameObject currentBullet = findBullet(chargeLevel);
        if (currentBullet == null) return;

        currentBullet.transform.position = bulletPoint.position;
        currentBullet.GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x), this, chargeLevel);
        StartCoroutine(ResetAttackCooldown());

    }

    private IEnumerator ResetAttackCooldown()
    {
        Attackable = false;
        yield return new WaitForSeconds(attackCooldown);
        Attackable = true;
    }

    private void resetAttack()
    {
        Attackable = true;
    }

    private void finishAttack()
    {
        playerMovement.movable = true;
        Attackable = true;  
        playerMovement.isAttacking = false;
    }

    private GameObject findBullet(int chargeLevel)
    {
        GameObject[] targetPool = normalBullets;
        if (chargeLevel == 1) targetPool = charge1Bullets;
        if (chargeLevel == 2) targetPool = charge2Bullets;

        for (int i = 0; i < targetPool.Length; i++)
        {
            if (!targetPool[i].activeInHierarchy)
            {
                return targetPool[i];
            }
        }
        return null;
    }

    public void reloadBullets()
    {
        if(bulletNumber < 3)
        {
            bulletNumber++;
            
        }
    }
}
