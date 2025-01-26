using UnityEngine;

public class Trapdoor : MonoBehaviour
{
    public bool PlayerIsNearby = false;
    public Sprite OpenSprite;
    public Sprite ClosedSprite;

    public float PlayerDistance = -1f;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        // Get SpriteRenderer
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = ClosedSprite;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            NotificationTextController.Instance.ShowNotification("Press E to open trapdoor");
            PlayerIsNearby = true;

            spriteRenderer.sprite = OpenSprite;
        }
    }
    
    
    //On Button Press E, if player is nearby, generate a new map
    void Update()
    {
        if (PlayerIsNearby)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player pressed E");
                // Generate a new map
                ProceduralMapGenerator.Instance.GenerateMap();
                PlayerIsNearby = false;
            }
            
            //Get Player Distance
            Vector2 playerPosition = PlayerStats.Instance.GetPlayerPosition();
            
            PlayerDistance = Vector2.Distance(playerPosition, transform.position);
            
            if (PlayerDistance > 10f)
            {
                NotificationTextController.Instance.ClearNotification();
                PlayerIsNearby = false;
                spriteRenderer.sprite = ClosedSprite;
            }
        }
    }

}
