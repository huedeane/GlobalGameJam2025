using System.Collections;
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
    public ProceduralMapGenerator ProceduralMapGenerator;
    public int NumberOfAttack = 10;
    public float DetectionRadius = .25f;
    public float DetectionGrowRate = .05f;
    public float StalkMoveRate = 10f;
    public float AttackRadius = 10f;
    public float AttackMoveRate = 15f;
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
        Agent.updateRotation = false; 
        AttackCollider.radius = AttackRadius;
        ProceduralMapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<ProceduralMapGenerator>();

        while (true)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            switch (AIState)
            {
                case FrogFishState.Idle:
                    AttackCollider.enabled = true;
                    DetectionCollider.radius += DetectionGrowRate * Time.deltaTime;
                    SpriteAnimatior.SetBool("IsMoving", false);
                    break;
                case FrogFishState.Stalk:
                    SpriteAnimatior.SetBool("IsMoving", true);
                    Agent.speed = StalkMoveRate;
                    break;
                case FrogFishState.Attack:
                    Agent.speed = AttackMoveRate;
                    SpriteAnimatior.SetBool("IsMoving", true);
                    break;
                case FrogFishState.Disoriented:
                    SpriteAnimatior.SetBool("IsMoving", false);
                    Agent.speed = 0;
                    yield return new WaitForSeconds(3f);
                    AIState = FrogFishState.RunAway;

                    break;
                case FrogFishState.RunAway:
                    SpriteAnimatior.SetBool("IsMoving", true);
                    AgentTarget = ProceduralMapGenerator.GetRandomFloorTileObject();
                    Agent.speed = AttackMoveRate * 2;
                    yield return new WaitUntil(() => Vector3.Distance(AgentTarget.transform.position, transform.position) <= 10f);
                    AIState = FrogFishState.ResetState;
                    break;
                case FrogFishState.ResetState:
                    DetectionCollider.radius = DetectionRadius;
                    DetectionCollider.enabled = true;
                    AttackCollider.enabled = true;
                    SpriteAnimatior.SetBool("IsMoving", false);
                    AIState = FrogFishState.Idle;
                    break;
            }

            yield return 0;
        }
    }

    private void Update()
    {
        if (AgentTarget != null) { 
            Vector3 direction = (AgentTarget.transform.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90f));
            }
            Agent.SetDestination(AgentTarget.transform.position);
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
        Debug.Log(AIState, collision);
        if (collision.CompareTag("Flashlight"))
        {
            AIState = FrogFishState.Disoriented;
        }

    }
}

