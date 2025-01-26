using System.Collections;
using Unity.VisualScripting;
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
    private GameObject AgentTarget;

    public Animator SpriteAnimatior;
    public NavMeshAgent Agent;
    public float WanderMoveRate = 10f;
    public float AttackMoveRate = 20f;
    public ProceduralMapGenerator ProceduralMapGenerator;
    public int Damage;


    IEnumerator Start()
    {
        //Initialize Value
        AIState = DeerFishState.Wander;
        ProceduralMapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<ProceduralMapGenerator>();

        while (true)
        {

            switch (AIState)
            {
                case DeerFishState.Wander:
                    yield return new WaitUntil(() => ProceduralMapGenerator.IsGenerationDone == true);

                    SpriteAnimatior.SetBool("IsMoving", true);
                    Agent.speed = WanderMoveRate;
                    AgentTarget = ProceduralMapGenerator.GetRandomFloorTileObject();
                    Agent.SetDestination(AgentTarget.transform.position);
                    yield return new WaitUntil(() => Vector3.Distance(AgentTarget.transform.position, transform.position) <= 5f || AIState == DeerFishState.Attack);
                    break;
                case DeerFishState.Attack:
                    Agent.SetDestination(AgentTarget.transform.position);
                    Agent.speed = AttackMoveRate;
                    break;
                case DeerFishState.Disoriented:
                    break;
                case DeerFishState.ResetState:

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerStats.Instance.CurrentOxygen -= Damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (AIState)
        {
            case DeerFishState.Wander:
                if (collision.CompareTag("Flashlight"))
                {
                    
                    AgentTarget = collision.GetComponentInParent<PlayerController>().GameObject();
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

