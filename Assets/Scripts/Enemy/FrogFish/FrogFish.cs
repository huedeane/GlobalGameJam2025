using System.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.AI;

public enum FrogFishState
{
    Idle,
    Stalk,
    Attack,
    Disoriented,
    RunAway,
    ResetState,
    Wait,
}

public class FrogFish : MonoBehaviour
{
    private FrogFishState AIState;
    private int CurrentTimesAttacked = 0;
    private GameObject AgentTarget;
    public int NumberOfAttack = 10;
    public float DetectionRadius = .25f;
    public float DetectionGrowRate = .05f;
    public float StalkMoveRate = .1f;
    public float AttackRadius = 10f;
    public float AttackMoveRate = 2f;
    public CircleCollider2D DetectionCollider;
    public CircleCollider2D CollisionCollider;
    public CircleCollider2D AttackCollider;
    public Animator SpriteAnimatior;
    public NavMeshAgent Agent;

    IEnumerator Start()
    {
        //Initialize Value
        AIState = FrogFishState.Idle;
        DetectionCollider.radius = DetectionRadius;
        AttackCollider.radius = AttackRadius;
        while (true)
        {
            switch (AIState)
            {
                case FrogFishState.Idle:
                    DetectionCollider.radius += DetectionGrowRate * Time.deltaTime;
                    SpriteAnimatior.SetBool("IsMoving", false);
                    break;
                case FrogFishState.Stalk:
                    SpriteAnimatior.SetBool("IsMoving", true);
                    Agent.SetDestination(AgentTarget.transform.position);
                    break;
                case FrogFishState.Attack:
                    CurrentTimesAttacked++;
                    SpriteAnimatior.SetBool("IsMoving", true);
                    break;
                case FrogFishState.Disoriented:
                    AIState = FrogFishState.RunAway;
                    SpriteAnimatior.SetBool("IsMoving", true);
                    break;
                case FrogFishState.RunAway:
                    SpriteAnimatior.SetBool("IsMoving", true);
                    break;
                case FrogFishState.ResetState:
                    DetectionCollider.radius = .25f;
                    DetectionCollider.enabled = true;
                    CurrentTimesAttacked = 0;
                    AIState = FrogFishState.Idle;
                    SpriteAnimatior.SetBool("IsMoving", false);
                    break;
            }
            Debug.Log(AIState);
            yield return 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (AIState) {
            case FrogFishState.Idle:
                if (collision.CompareTag("Player"))
                {
                    DetectionCollider.enabled = false;
                    AIState = FrogFishState.Stalk;
                    AgentTarget = collision.gameObject;
                }
                break;
            case FrogFishState.Stalk:
                if (collision.CompareTag("Player"))
                {
                    AttackCollider.enabled = false;
                    AIState = FrogFishState.Attack;
                }
                break;
        }

        if (collision.CompareTag("Bubble")) 
        {
            AIState = FrogFishState.Disoriented;
        }

    }
}

