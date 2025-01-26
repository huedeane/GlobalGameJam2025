using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryHandler : MonoBehaviour
{
    // Singleton
    public static PlayerInventoryHandler Instance;
    
    [SerializeField]
    public InventorySprite[] InventorySprites;
    
    [Serializable]
    public struct InventorySprite
    {
        public PlayerStats.ItemType ItemType;
        public Sprite ItemSprite;
    }
    
    public GameObject[] SlotObjects = new GameObject[5];

    // Padding for items inside slots
    public float padding = 10f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Initialize SlotObjects by grabbing children
        for (int i = 0; i < SlotObjects.Length; i++)
        {
            SlotObjects[i] = transform.GetChild(i).gameObject;

            // Give each slot its own child GameObject
            GameObject slotChild = new GameObject("SlotChild");
            slotChild.transform.SetParent(SlotObjects[i].transform, false); // Use SetParent with worldPositionStays = false
            slotChild.transform.localPosition = Vector3.zero;

            // Add an Image component to SlotChild
            Image childImage = slotChild.AddComponent<Image>();
            childImage.raycastTarget = false;

            // Set RectTransform properties
            RectTransform childRect = slotChild.GetComponent<RectTransform>();
            childRect.anchorMin = Vector2.zero; 
            childRect.anchorMax = Vector2.one; 
            childRect.offsetMin = new Vector2(padding, padding);  // Add padding to the bottom-left corner
            childRect.offsetMax = new Vector2(-padding, -padding); // Add padding to the top-right corner
            
            // Create a child for the dark overlay
            GameObject darkOverlay = new GameObject("DarkOverlay");
            darkOverlay.transform.SetParent(SlotObjects[i].transform, false);

            // Add an Image component to the overlay
            Image overlayImage = darkOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.8f); 
            overlayImage.raycastTarget = false; 
            overlayImage.enabled = false;

            RectTransform overlayRect = darkOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero; 
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
        }

        SyncInventorySprites();
    }

    public void SyncInventorySprites()
    {
        int i = 0;
        foreach (GameObject slot in SlotObjects)
        {
            // Get the Image component of the child object
            Image slotImage = slot.transform.GetChild(0).GetComponent<Image>();
            
            // Get the ItemType of the slot
            PlayerStats.ItemType slotItemType = PlayerStats.Instance.Inventory[i];

            // Find the InventorySprite that matches the ItemType
            InventorySprite inventorySprite = Array.Find(InventorySprites, x => x.ItemType == slotItemType);
            
            // Assign the sprite or clear it
            if (inventorySprite.ItemSprite != null)
            {
                slotImage.sprite = inventorySprite.ItemSprite;
                slotImage.enabled = true; // Enable the Image if there's a sprite
            }
            else
            {
                slotImage.sprite = null;
                slotImage.enabled = false; // Hide the Image if there's no sprite
            }
            
            if (i == PlayerStats.Instance.CurrentInventorySlot)
            {
                HighlightSlot(slot);
            }
            else
            {
                ResetSlotHighlight(slot);
            }

            i++;
        }
    }
    
    public void HighlightSlot(GameObject slot)
    {
        Transform overlay = slot.transform.Find("DarkOverlay");
        if (overlay != null)
        {
            overlay.GetComponent<Image>().enabled = true;
        }
    }
    
    public Sprite GetSpriteForItemType(PlayerStats.ItemType itemType)
    {
        InventorySprite inventorySprite = Array.Find(InventorySprites, x => x.ItemType == itemType);
        return inventorySprite.ItemSprite;
    }

    public void ResetSlotHighlight(GameObject slot)
    {
        Transform overlay = slot.transform.Find("DarkOverlay");
        if (overlay != null)
        {
            overlay.GetComponent<Image>().enabled = false;
        }
    }
}
