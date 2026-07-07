using UnityEngine;

public class EnergyRegenItem : Item
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

        return; // Energy regeneration effect is not implemented yet
    }
}
