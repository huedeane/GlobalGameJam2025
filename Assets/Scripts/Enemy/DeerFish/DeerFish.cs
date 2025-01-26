using System.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public enum DeerFishState
{
    Wander,
    Attack,
    Disoriented,
    ResetState,
}

public class DeerFish : MonoBehaviour
{
    private DeerFishState AIState;
    private int CurrentTimesAttacked = 0;

    public CircleCollider2D DetectionCollider;
    public float DetectionRadius = .25f;
    public int NumberOfAttack = 10;


    IEnumerator Start()
    {
        //Initialize Value
        AIState = DeerFishState.Wander;
        DetectionCollider.radius = DetectionRadius;

        while (true)
        {
            switch (AIState)
            {
                case DeerFishState.Wander:
                    break;
                case DeerFishState.Attack:
                    CurrentTimesAttacked++;
                    break;
                case DeerFishState.Disoriented:
                    break;
                case DeerFishState.ResetState:
                    CurrentTimesAttacked = 0;
                    AIState = DeerFishState.Wander;
                    break;
            }
            yield return 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (AIState)
        {
            case DeerFishState.Wander:
                if (collision.CompareTag("Flashlight"))
                {
                    AIState = DeerFishState.Attack;
                }
                break;
        }

        if (collision.CompareTag("Bubble"))
        {
            AIState = DeerFishState.Disoriented;
        }

    }
}

