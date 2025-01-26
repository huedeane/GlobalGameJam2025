using TMPro;
using UnityEngine;

public class MoneyTracker : MonoBehaviour
{
    public TextMeshProUGUI CurrentMoney;
    
    // Update is called once per frame
    void Update()
    {
        CurrentMoney.text = "$" + PlayerStats.Instance.CurrentMoney.ToString();

    }


}
