using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;

    

    private void Start()
    {
        totalHealthBar.fillAmount = playerHealth.currentHp / 32;
    }

    private void Update()
    {
        currentHealthBar.fillAmount = playerHealth.currentHp / 32;
    }
}