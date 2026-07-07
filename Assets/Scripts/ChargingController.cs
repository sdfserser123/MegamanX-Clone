using UnityEngine;

public class ChargingController : MonoBehaviour
{
    private Animator animator;

    public bool isCharging = false;
    public float chargeLevel = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("charge", isCharging);
        animator.SetFloat("ChargingLevel", chargeLevel);
    }
}
