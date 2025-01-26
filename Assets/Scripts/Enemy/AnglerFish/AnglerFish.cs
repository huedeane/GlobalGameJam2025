using System.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public enum AnglerFishState
{
    Idle,
    Emerge,
    Attack,
    Disoriented,
    ResetState,
    Wait,
}

public class AnglerFish : MonoBehaviour
{
    private AnglerFishState AIState;
    private int CurrentTimesAttacked = 0;
    
    public CircleCollider2D DetectionCollider;
    public float DetectionRadius = .25f;
    public int NumberOfAttack = 10;

    IEnumerator Start()
    {

        //Initialize Value
        AIState = AnglerFishState.Idle;
        DetectionCollider.radius = DetectionRadius;

        while (true)
        {
            switch (AIState)
            {
                case AnglerFishState.Idle:
                    break;
                case AnglerFishState.Emerge:
                    AIState = AnglerFishState.Attack;
                    break;
                case AnglerFishState.Attack:
                    CurrentTimesAttacked++;
                    break;
                case AnglerFishState.Disoriented:
                    break;
                case AnglerFishState.ResetState:
                    CurrentTimesAttacked = 0;
                    break;
            }
            yield return 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            AIState = AnglerFishState.Emerge;
        }
    }
}

