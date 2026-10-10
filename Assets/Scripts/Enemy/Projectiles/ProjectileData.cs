using UnityEngine;

public enum ProjectileType
{
    Bullet,
    Snowball
}

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [Header("Projectile Type Selection")]
    public ProjectileType projectileType;

    [Header("General Settings")]
    public string projectileName;
    public Sprite projectileSprite;
    public RuntimeAnimatorController animatorController;

    [Header("Value Settings")]
    public float speed;
    public float damage;
    public float lifetime;
}
