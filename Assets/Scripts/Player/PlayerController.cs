using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] walkingForwardFrames;
    public Sprite[] idleFrames;
    public float frameInterval = 0.1f; // Interval (in seconds) between each frame update

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private float animationTimer;
    private int currentFrame;

    public GameObject[] FlashlightObjects;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Capture player movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // Handle scroll wheel first
        HandleMouseScroll();

        // Rotate towards mouse
        RotateTowardsMouse();
    }

    void FixedUpdate()
    {
        // Apply velocity
        rb.linearVelocity = movement * PlayerStats.Instance.MoveSpeed;

        // Update sprite animation
        HandleAnimationFrames();
    }

    /// <summary>
    /// Rotates the character so it faces the mouse cursor in 2D.
    /// </summary>
    void RotateTowardsMouse()
    {
        // Get mouse position in world coordinates
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Ensure z is zero since we're in 2D

        // Calculate direction from player to mouse
        Vector3 direction = mousePos - transform.position;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float angleOffset = 90f; // Offset the angle to match the sprite's forward direction

        // Apply the rotation around the Z axis
        transform.rotation = Quaternion.AngleAxis(angle + angleOffset, Vector3.forward);
    }

    void HandleAnimationFrames()
    {
        // Increase our timer
        animationTimer += Time.fixedDeltaTime;

        // If movement is detected, cycle through walking frames
        if (movement.magnitude > 0.01f)
        {
            if (animationTimer >= frameInterval)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % walkingForwardFrames.Length;
                spriteRenderer.sprite = walkingForwardFrames[currentFrame];
            }
        }
        // Otherwise, use idle frames
        else
        {
            if (animationTimer >= frameInterval)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % idleFrames.Length;
                spriteRenderer.sprite = idleFrames[currentFrame];
            }
        }
    }
    
    private void HandleMouseScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            Debug.Log("Scrolling Up: Adding 1 to inventory slot");
            PlayerStats.Instance.OnItemChange(-1);
        }
        else if (scroll < 0f)
        {
            Debug.Log("Scrolling Down: Subtracting 1 from inventory slot");
            PlayerStats.Instance.OnItemChange(1);
        }
        
        bool flashlightActive = PlayerStats.Instance.GetCurrentItem() == PlayerStats.ItemType.FlashLight;

        ToggleFlashlight(flashlightActive);
            
    }
    
    public void ToggleFlashlight(bool flashlightActive)
    {
        foreach (GameObject flashlight in FlashlightObjects)
        {
            flashlight.SetActive(flashlightActive);
        }
    }
}
