using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CollectibleItem : MonoBehaviour
{
    public PlayerStats.ItemType ItemType;
    public string FriendlyName;
    public Sprite ItemSprite;

    public int Weight = 5;
    public int value = 5;

    public bool CanBeThrown = false;

    public bool IsIlluminated = true;
    public float LightRadius = 10f;
    public Color LightColor = new Color(255, 214, 0, 255);

    public float sizePercentage = 10f;

    void Start()
    {
        // Get SpriteRenderer
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        
        // Get ItemSprite for this type
        ItemSprite = PlayerInventoryHandler.Instance.GetSpriteForItemType(ItemType);
        
        if (ItemSprite != null)
        {
            spriteRenderer.sprite = ItemSprite;

            // Resize sprite without affecting the collider
            ResizeSprite(spriteRenderer);
        }

        // Generate a FriendlyName if not set
        if (string.IsNullOrEmpty(FriendlyName))
        {
            // Parse the ItemType to get a human-readable name
            FriendlyName = System.Text.RegularExpressions.Regex.Replace(ItemType.ToString(), "(\\B[A-Z])", " $1");
        }

        // Add illumination if applicable
        if (IsIlluminated)
        {
            Light2D light = gameObject.AddComponent<Light2D>();
            light.pointLightOuterRadius = LightRadius;
            light.intensity = .3f;
            light.color = LightColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collided with " + other.gameObject.name);
        Debug.Log("Collided with Tag " + other.gameObject.tag);

        if (other.gameObject.CompareTag("Player"))
        {
            bool isAdded = PlayerStats.Instance.AttemptToAddItemToInventory(ItemType);
            if (isAdded) Destroy(gameObject);
        }
    }

    private void ResizeSprite(SpriteRenderer spriteRenderer)
    {
        // Get the original size of the sprite
        Vector2 originalSize = spriteRenderer.sprite.bounds.size;

        // Calculate the new size based on the sizePercentage
        Vector2 targetSize = originalSize * (sizePercentage / 100f);

        // Calculate the scale factors
        float scaleX = targetSize.x / originalSize.x;
        float scaleY = targetSize.y / originalSize.y;

        // Apply the scale factors to the SpriteRenderer's transform
        spriteRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1);

        // Leave the Collider's scale unaffected
        Collider2D collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            // Reset collider's offset and scale to prevent unintended changes
            collider.offset = collider.offset; // Keeps the same offset
        }
    }
}
