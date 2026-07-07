using UnityEngine;

public class ItemObject : Item
{
    [Header("Data Configuration")]
    [SerializeField] private ItemData data;

    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();

        if (data != null)
        {
            SetupItem(data);
        }
    }

    public void SetupItem(ItemData newData)
    {
        data = newData;

        if (spriteRenderer != null && data.itemSprite != null)
        {
            spriteRenderer.sprite = data.itemSprite;

            if (physicsCollider != null) physicsCollider.size = data.itemSprite.bounds.size;
            if (triggerCollider != null) triggerCollider.size = data.itemSprite.bounds.size;
        }

        if (animator != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }
    }

    protected override void ApplyEffect(GameObject player)
    {
        if (data == null) return;

        switch(data.itemType)
        {
            case ItemType.Healing:
                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(data.valueAmount);
                }
                break;
            case ItemType.EnergyRegen:
                // Implement energy regeneration effect here
                break;
            case ItemType.LifeUp:
                // Implement life up effect here
                break;
            case ItemType.HeartTank:
                // Implement heart tank effect here
                break;
            default:
                Debug.LogWarning("Unhandled item type: " + data.itemType);
                break;
        }

        
    }
}