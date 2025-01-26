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
        Money,
        Flashbang,
        AlienStatue,
        OxygenGun,
        Flippers,
        OxygenTank,
        Battery,
        None
    }
    
    public int CurrentInventorySlot = 0;
    
    public int InventorySize = 6;

    public int MaxOxygen = 100;
    public int CurrentOxygen = 100;

    public int CurrentEnergy = 100;
    public int MaxEnergy = 100;

    public int MoveSpeed = 20;

    public int CurrentMoney = 0;
    public int Quota = 1000;
    public GameObject GameOver;
    
    [SerializeField] public ItemType[] Inventory;
    
    public static PlayerStats Instance;
    
    private int frameCounter = 0; // Counter to track frames
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

    public Vector2 GetPlayerPosition()
    {
        //Find the player object
        GameObject player = GameObject.Find("Player");
        
        
        //Return the player's position
        return player.transform.position;
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

    public void FixedUpdate()
    {
        // Increment the frame counter
        frameCounter++;

        // Check if Flashlight is equipped
        if (GetCurrentItem() == ItemType.FlashLight)
        {
            // Decrement energy every 15 frames
            if (frameCounter >= 15)
            {
                CurrentEnergy -= 3; // Reduce energy
                frameCounter = 0;   // Reset the counter
                CurrentOxygen -= 2; // Reduce oxygen
            }
        }
        else
        {
            // Reset the counter if Flashlight is not equipped
            frameCounter = 0;
        }
    }

    public void OnItemChange(int movementAmount = 1, ItemType newType = ItemType.BubbleGun)
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
        
        if(movementAmount == 0 && newType != ItemType.BubbleGun)
        {
            newItem = newType;
        }
        
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
            case ItemType.Money:
                break;
            case ItemType.Flashbang:
                break;
            case ItemType.AlienStatue:
                break;
            case ItemType.OxygenGun:
                break;
            case ItemType.Flippers:
                MoveSpeed = MoveSpeed/2;
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
            case ItemType.Money:
                break;
            case ItemType.Flashbang:
                break;
            case ItemType.AlienStatue:
                break;
            case ItemType.OxygenGun:
                break;
            case ItemType.Flippers:
                MoveSpeed = MoveSpeed*2;
                break;
            case ItemType.OxygenTank:
                break;
            case ItemType.None:
                break;
        }
        
        //Bubble gun is never going to be added, so if it's anything but a bubble gun, we can swap the item
        
        CurrentInventorySlot = newSlot;
        
        if (newType != ItemType.BubbleGun)
        {
            Inventory[CurrentInventorySlot] = newType;
        }
        
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

    public void SetCurrentItem(ItemType type)
    {
        OnItemChange(0, type);


    }
    
    public int GetItemValue(ItemType type)
    {
        switch (type)
        {
            case ItemType.BubbleGun:
                return 0;
            case ItemType.FlashLight:
                return 0;
            case ItemType.Bomb:
                return 50;
            case ItemType.Whistle:
                return 50;
            case ItemType.Money:
                return 300;
            case ItemType.Flashbang:
                return 50;
            case ItemType.AlienStatue:
                return 100;
            case ItemType.OxygenGun:
                return 0;
            case ItemType.Battery:
                return 150;
            case ItemType.Flippers:
                return 50;
            case ItemType.OxygenTank:
                return 0;
            case ItemType.None:
                return 0;
            default:
                return 0;
        }
    }

    public void Update()
    {
        if (CurrentOxygen <= 0)
        {
            GameOver.SetActive(true);
        }
    }


}