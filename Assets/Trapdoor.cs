using UnityEngine;

public class Trapdoor : MonoBehaviour
{
    public bool PlayerIsNearby = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            NotificationTextController.Instance.ShowNotification("Press E to open trapdoor");
            PlayerIsNearby = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            NotificationTextController.Instance.ClearNotification();
            PlayerIsNearby = false;

        }
    }
    
    //On Button Press E, if player is nearby, generate a new map
    void Update()
    {
        if (PlayerIsNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player pressed E");
            // Generate a new map
            ProceduralMapGenerator.Instance.GenerateMap();
            PlayerIsNearby = false;
        }
    }

}
