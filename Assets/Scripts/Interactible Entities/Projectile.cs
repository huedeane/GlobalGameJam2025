using UnityEngine;

public class Projectile : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    // Modifiers
    //---------------------------------------------------------------------------------------------//
   
    [Header("Sine Wave")]
    public bool MoveOnSineWave = false;
    
    public float SineAmplitude = 0.5f;
    public float SineFrequency = 1f;
    
    [Header("General Projectile Settings")]
    public float Speed = .2f; 
    public float MinSpeed = 0.05f;

    
    public float ProjectileSpeedDampening = .99f;
    
    public float LifeSpanInSeconds = 5f;

    private Vector2 TargetPosition = Vector2.zero;
    public bool MovementStarted = false;
    
    public void SetTargetPosition(Vector2 targetPosition)
    {
        TargetPosition = targetPosition;
        MovementStarted = true;
    }
    

    private void FixedUpdate()
    {
        if (!MovementStarted)
            return;

        if (TargetPosition == Vector2.zero)
        {
            MovementStarted = false;
        }

        if (MoveOnSineWave)
        {
            SineWaveMovement();
        }
        else
        {
            LinearMovement();
        }

        LifeSpanInSeconds -= Time.deltaTime;

        // Apply dampening but clamp to a minimum speed
        Speed = Mathf.Max(Speed * ProjectileSpeedDampening, MinSpeed);

        if (LifeSpanInSeconds <= 0)
        {
            Destroy(gameObject);
        }
    }

    
    private void LinearMovement()
    {
        transform.position = Vector2.MoveTowards(transform.position, TargetPosition, Speed);
    }
    
    private void SineWaveMovement()
    {
        // Calculate the direction vector from current position to the target
        Vector2 direction = (TargetPosition - (Vector2)transform.position).normalized;

        // Move in the direction of the target position, but add sine wave offset perpendicular to the direction
        float sineOffset = SineAmplitude * Mathf.Sin(Time.time * SineFrequency);
        Vector2 perpendicularDirection = new Vector2(-direction.y, direction.x); // Perpendicular to the movement direction

        Vector2 newPosition = Vector2.MoveTowards(transform.position, TargetPosition, Speed) + perpendicularDirection * sineOffset;
        transform.position = newPosition;
    }
    
    

}
