using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // Movement speed
    public float moveSpeed = 2f;

    // Reference to the Rigidbody2D component
    private Rigidbody2D rb;

    // Input variables
    private Vector2 movement;

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get movement input
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow Keys
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down Arrow Keys

        // Normalize the movement vector to prevent faster diagonal movement
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        // Apply movement based on Rigidbody2D
        rb.linearVelocity = movement * moveSpeed;
    }
}