using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private float damaged;
    private PlayerAttack playerAttack;
    
    private float direction;
    private bool hit;
    private BoxCollider2D boxCollider;
    private Animator animator;
    private int myChargeLevel;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hit)
        {
            return;
        }

        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((whatIsTarget.value & (1 << collision.gameObject.layer)) > 0)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                float HpBeforeHit = enemy.CurrHp;

                if (damaged == 1)
                {
                    HandleExplosion();
                }

                else if (damaged > 1 && collision.CompareTag("Boss") && damaged < HpBeforeHit)
                {
                    HandleExplosion();
                }
                enemy.GetHit(damaged);
            }
            else
            {
                HandleExplosion();
            }
            
        }
    }
    private void HandleExplosion()
    {
        hit = true;
        boxCollider.enabled = false;
        animator.SetTrigger("explode");
    }

    public void SetDirection(float _direction, PlayerAttack _playerAttack, int chargeLevel)
    {
        playerAttack = _playerAttack;
        direction = _direction;
        myChargeLevel = chargeLevel;

        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _direction)
        {
            localScaleX = -localScaleX;
        }

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnBecameInvisible()
    {
        Deactivate();
        playerAttack.reloadBullets();
    }
}
