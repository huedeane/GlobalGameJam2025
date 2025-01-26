using TMPro;
using UnityEngine;

public class NotificationTextController : MonoBehaviour
{
    public TextMeshProUGUI NotificationText;

    public int timer;
    
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

    void Start()
    {
        NotificationText.text = "";
        timer = -1;
    }
    
    void FixedUpdate()
    {
        if (timer > 0)
        {
            timer--;
        }
        else if (timer == 0)
        {
            ClearNotification();
            timer = -1;
        }
    }
    
    public void ShowNotification(string message, int duration = -1) 
    {
        NotificationText.text = message;
        
        //Duration in seconds
        timer = duration * 60;
    }
    
    public void ClearNotification()
    {
        NotificationText.text = "";
    }
    

}
