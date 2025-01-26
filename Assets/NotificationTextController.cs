using TMPro;
using UnityEngine;

public class NotificationTextController : MonoBehaviour
{
    public TextMeshProUGUI NotificationText;
    
    public static NotificationTextController Instance;
    
    private void Awake()
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
    
    public void ShowNotification(string message)
    {
        NotificationText.text = message;
    }
    
    public void ClearNotification()
    {
        NotificationText.text = "";
    }
    

}
