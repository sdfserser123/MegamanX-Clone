using UnityEngine;

public class SpriteBlink : MonoBehaviour
{
    [SerializeField] private float flashingSpeed = 0.5f;

    private SpriteRenderer spriteRenderer;

    public bool isFlashing = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFlashing)
        {
            float flashValue = (Time.time % (flashingSpeed * 2) > flashingSpeed) ? 0.6f : 0f;

            spriteRenderer.material.SetFloat("_FlashIntensity", flashValue);
        }
        else
        {
            spriteRenderer.material.SetFloat("_FlashIntensity", 0f);
        }
    }
}
