using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CollectibleItem : MonoBehaviour
{
    public Sprite ItemIcon;
    public string ItemName;
    
    public int Weight = 5;
    public int value = 5;

    public bool CanBeThrown = false;

    public bool IsIlluminated = true;
    public float LightRadius = 10f;
    public Color LightColor = new Color(255, 214, 0, 255);
    
    void Start()
    {
        if (IsIlluminated)
        {
            Light2D light = gameObject.AddComponent<Light2D>();
            light.pointLightOuterRadius = LightRadius;
            light.intensity = .3f;
            light.color = LightColor;
        }
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collided with " + other.gameObject.name);
        Debug.Log("Collided with Tag" + other.gameObject.tag);

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Collected " + ItemName);
            Destroy(gameObject);
        }
    }
}
