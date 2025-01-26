using System;
using UnityEngine;

public class PlayerShootBehavior : MonoBehaviour
{
    public GameObject DefaultBubblePrefab;

    private void Update()
    {
        PlayerStats.ItemType currentItem = PlayerStats.Instance.GetCurrentItem();

        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            HandleLeftClick(currentItem);
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click
        {
            HandleRightClick(currentItem);
        }
    }

    private void HandleLeftClick(PlayerStats.ItemType currentItem)
    {
        switch (currentItem)
        {
            case PlayerStats.ItemType.BubbleGun:
                ShootBubble();
                break;

            case PlayerStats.ItemType.OxygenTank:
                UseOxygenTank();
                break;
            
            case PlayerStats.ItemType.Battery:
                UseBattery();
                break;

            default:
                NotificationTextController.Instance.ShowNotification("Right click to sell item", 2);
                break;
        }
    }

    private void HandleRightClick(PlayerStats.ItemType currentItem)
    {
        switch (currentItem)
        {
            case PlayerStats.ItemType.FlashLight:
                ToggleFlashlight();
                break;

            default:
                SellItem(currentItem);
                break;
        }
    }

    private void ShootBubble()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPosition = transform.position;
        Vector2 direction = (mousePosition - playerPosition).normalized;

        GameObject bubble = Instantiate(DefaultBubblePrefab, playerPosition, Quaternion.identity);
        bubble.GetComponent<Projectile>().SetTargetPosition(playerPosition + direction * 1000);
        PlayerStats.Instance.CurrentOxygen -= 10;
    }

    private void UseOxygenTank()
    {
        PlayerStats.Instance.SetCurrentItem(PlayerStats.ItemType.None);
        PlayerStats.Instance.CurrentOxygen = Math.Min(
            PlayerStats.Instance.CurrentOxygen + PlayerStats.Instance.MaxOxygen / 3,
            PlayerStats.Instance.MaxOxygen
        );
    }
    
    private void UseBattery()
    {
        PlayerStats.Instance.SetCurrentItem(PlayerStats.ItemType.None);
        PlayerStats.Instance.CurrentEnergy = Math.Min(
            PlayerStats.Instance.CurrentEnergy + PlayerStats.Instance.MaxEnergy / 2,
            PlayerStats.Instance.MaxEnergy
        );
    }

    private void SellItem(PlayerStats.ItemType currentItem)
    {
        int itemValue = PlayerStats.Instance.GetItemValue(currentItem);
        if (itemValue > 0)
        {
            PlayerStats.Instance.SetCurrentItem(PlayerStats.ItemType.None);
            PlayerStats.Instance.CurrentMoney += itemValue;
        }
    }

    private void ToggleFlashlight()
    {
        // Placeholder logic for toggling the flashlight
        Debug.Log("Flashlight toggled");
    }

    private void OnDrawGizmos()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPosition = transform.position;
        Vector2 direction = (mousePosition - playerPosition).normalized;

        Gizmos.DrawLine(playerPosition, playerPosition + direction * 100);
    }
}
