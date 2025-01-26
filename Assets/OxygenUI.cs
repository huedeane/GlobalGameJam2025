using UnityEngine;

public class OxygenUI : MonoBehaviour
{
    private RectTransform RectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 size = RectTransform.sizeDelta;
        size.y = PlayerStats.Instance.CurrentOxygen;
        RectTransform.sizeDelta = size;
    }
}
