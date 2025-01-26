using UnityEngine;

public class PlayerShootBehavior : MonoBehaviour
{
    
    public GameObject DefaultBubblePrefab;

    //On Left Mouse Button Click, shoot a bubble in the direction of the mouse
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && PlayerStats.Instance.GetCurrentItem() == PlayerStats.ItemType.BubbleGun)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 playerPosition = transform.position;

            Vector2 direction = (mousePosition - playerPosition).normalized;

            GameObject bubble = Instantiate(DefaultBubblePrefab, playerPosition, Quaternion.identity);
            bubble.GetComponent<Projectile>().SetTargetPosition(playerPosition + direction * 1000);
            PlayerStats.Instance.CurrentOxygen -= 10;
        }
    }
    
    //OnDrawGizmos, draw a line from the player to the mouse position using the same implementation as above
    private void OnDrawGizmos()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPosition = transform.position;

        Vector2 direction = (mousePosition - playerPosition).normalized;

        Gizmos.DrawLine(playerPosition, playerPosition + direction * 100);
    }
}
