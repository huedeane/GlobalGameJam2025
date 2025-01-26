using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Serializable]
    public enum ItemType
    {
        BubbleGun,
        FlashLight,
        Bomb,
        Whistle,
        MoneyGun,
        Flashbang,
        AlienStatue,
        OxygenGun,
        Flippers,
        OxygenTank,
        None
    }
    
    public int InventorySize = 5;

    public int Oxygen = 100;
    public int CurrentOxygen = 100;
    
    [SerializeField] public ItemType[] Inventory;
    
    public static PlayerStats Instance;
    
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





}
