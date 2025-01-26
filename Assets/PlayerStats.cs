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
    
    public int CurrentInventorySlot = 0;
    
    public int InventorySize = 5;

    public int MaxOxygen = 100;
    public int CurrentOxygen = 100;

    public int MoveSpeed = 20;
    
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

    private void Start()
    {
        //Check if Inventory has been initialized
        if (Inventory == null)
        {
            Inventory = new ItemType[InventorySize];

            //Initialize Inventory with Bubble Gun at Index 0 and Flashlight at Index 1
            Inventory[0] = ItemType.BubbleGun;
            Inventory[1] = ItemType.FlashLight;

            //Fill the rest with None
            for (int i = 2; i < InventorySize; i++)
            {
                Inventory[i] = ItemType.None;
            }
        }
    }

    public void OnItemChange(int movementAmount = 1)
    {
        int newSlot = -1;
        if(CurrentInventorySlot + movementAmount < 0 || CurrentInventorySlot + movementAmount >= InventorySize)
        {
            //Loop around
            newSlot = (CurrentInventorySlot + movementAmount + InventorySize) % InventorySize;
        }
        else
        {
            newSlot = CurrentInventorySlot + movementAmount;
        }
        
        ItemType oldItem = Inventory[CurrentInventorySlot];
        ItemType newItem = Inventory[newSlot];
        
        //Logic for Unequipping old item if necessary
        switch (oldItem)
        {
            case ItemType.BubbleGun:
                break;
            case ItemType.FlashLight:
                break;
            case ItemType.Bomb:
                break;
            case ItemType.Whistle:
                break;
            case ItemType.MoneyGun:
                break;
            case ItemType.Flashbang:
                break;
            case ItemType.AlienStatue:
                break;
            case ItemType.OxygenGun:
                break;
            case ItemType.Flippers:
                break;
            case ItemType.OxygenTank:
                break;
            case ItemType.None:
                break;
        }
        
        //Logic for Equipping new item if necessary
        switch (newItem)
        {
            case ItemType.BubbleGun:
                break;
            case ItemType.FlashLight:
                break;
            case ItemType.Bomb:
                break;
            case ItemType.Whistle:
                break;
            case ItemType.MoneyGun:
                break;
            case ItemType.Flashbang:
                break;
            case ItemType.AlienStatue:
                break;
            case ItemType.OxygenGun:
                break;
            case ItemType.Flippers:
                break;
            case ItemType.OxygenTank:
                break;
            case ItemType.None:
                break;
        }
        
        CurrentInventorySlot = newSlot;
        
        PlayerInventoryHandler.Instance.SyncInventorySprites();
    }

    public ItemType GetCurrentItem()
    {
        return Inventory[CurrentInventorySlot];
    }
    
    public bool AttemptToAddItemToInventory(ItemType type)
    {
        for (int i = 0; i < InventorySize; i++)
        {
            if (Inventory[i] == ItemType.None)
            {
                Inventory[i] = type;
                PlayerInventoryHandler.Instance.SyncInventorySprites();
                return true;
            }
        }

        return false;
    }

    
    
}