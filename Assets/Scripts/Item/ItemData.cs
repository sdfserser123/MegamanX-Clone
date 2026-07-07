using UnityEngine;

public enum ItemType
{
    Healing,
    EnergyRegen,
    LifeUp,
    HeartTank
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Type Selection")]
    public ItemType itemType;

    [Header("General Settings")]
    public string itemName;
    public Sprite itemSprite;
    public RuntimeAnimatorController animatorController;
    public bool dropable;
    [Header("Value Settings")]
    public int valueAmount;
}