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

    public int MaxOxygen = 310;
    public int CurrentOxygen = 310;
    
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
