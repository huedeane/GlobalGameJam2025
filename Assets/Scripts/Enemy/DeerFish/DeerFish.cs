using System;
using System.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.AI;

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
    private GameObject AgentTarget;

    public Animator SpriteAnimatior;
    public NavMeshAgent Agent;
    public CircleCollider2D DetectionCollider;
    public float DetectionRadius = .25f;
    public int NumberOfAttack = 10;
    public ProceduralMapGenerator ProceduralMapGenerator;


    IEnumerator Start()
    {
        //Initialize Value
        AIState = DeerFishState.Wander;
        DetectionCollider.radius = DetectionRadius;
        ProceduralMapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<ProceduralMapGenerator>();

        //Log whether the agent is on the navmesh
        Debug.Log("Is On NavMesh: " + Agent.isOnNavMesh);
        //Log the agent's destination
        Debug.Log(Agent.destination);
        //Log whether the agent is blocked 
        Debug.Log(Agent.isPathStale);

        while (true)
        {

            switch (AIState)
            {
                case DeerFishState.Wander:
                    yield return new WaitUntil(() => ProceduralMapGenerator.IsGenerationDone == true);

                    SpriteAnimatior.SetBool("IsMoving", true);

                    AgentTarget = ProceduralMapGenerator.GetRandomFloorTileObject();
                    Agent.SetDestination(AgentTarget.transform.position);
                    Debug.Log("Found Floor at: " + AgentTarget.transform.position); ;
                    yield return new WaitUntil(() => Vector3.Distance(AgentTarget.transform.position, transform.position) <= 5f);
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

    public void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (AgentTarget != null) {
            Vector3 direction = (AgentTarget.transform.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90f));
            }
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

