using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

public enum AnglerFishState
{
    Idle,
    Emerge,
    Attack,
    Disoriented,
    ResetState,
    Wait,
    Wander,
}

public class AnglerFish : MonoBehaviour
{
    private AnglerFishState AIState;
    public Animator SpriteAnimatior;
    public Light2D AnglerIdleLight;
    public Light2D AnglerEmergeLight;
    private GameObject AgentTarget;
    public NavMeshAgent Agent;
    public CircleCollider2D DetectionCollider;
    public CircleCollider2D AttackCollider;
    public int Damage;
    public int MaxHealth;
    public int Health;
    public float MovementSpeed;
    public float AttackRadius;
    public ProceduralMapGenerator ProceduralMapGenerator;

    IEnumerator Start()
    {

        //Initialize Value
        AIState = AnglerFishState.Idle;
        ProceduralMapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<ProceduralMapGenerator>();

        while (true)
        {
            switch (AIState)
            {
                case AnglerFishState.Idle:
                    AnglerIdleLight.enabled = true;
                    AnglerEmergeLight.enabled = false;
                    AttackCollider.radius = 0;
                    break;
                case AnglerFishState.Emerge:
                    SpriteAnimatior.SetBool("IsEmerge", true);
                    DetectionCollider.radius = 0;
                    yield return new WaitUntil(() =>
                    {
                        AnimatorStateInfo stateInfo = SpriteAnimatior.GetCurrentAnimatorStateInfo(0);
                        return stateInfo.IsName("EmergeMove") && stateInfo.normalizedTime >= 1.0f;
                    });
                    AnglerIdleLight.enabled = false;
                    AnglerEmergeLight.enabled = true;

                    AIState = AnglerFishState.Attack;
                    break;
                case AnglerFishState.Attack:
                    Agent.SetDestination(AgentTarget.transform.position);
                    break;
                case AnglerFishState.Disoriented:
                    SpriteAnimatior.SetBool("IsMoving", false);
                    Agent.speed = 0;
                    yield return new WaitForSeconds(1f);
                    AIState = AnglerFishState.Wander;
                    break;
                case AnglerFishState.Wander:
                    Agent.speed = MovementSpeed;
                    AttackCollider.radius = AttackRadius;
                    Health = MaxHealth;
                    SpriteAnimatior.SetBool("IsMoving", true);
                    AgentTarget = ProceduralMapGenerator.GetRandomFloorTileObject();
                    Agent.SetDestination(AgentTarget.transform.position);
                    yield return new WaitUntil(() => Vector3.Distance(AgentTarget.transform.position, transform.position) <= 30f || AIState != AnglerFishState.Wander);
                    break;
            }

            yield return 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (AIState == AnglerFishState.Attack && collision.gameObject.CompareTag("Player")) { 
            PlayerStats.Instance.CurrentOxygen -= Damage;
        }
    }


    public void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (AgentTarget != null)
        {
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
        if (collision.CompareTag("Player") && AIState == AnglerFishState.Idle) {
            AIState = AnglerFishState.Emerge;
            AgentTarget = collision.GameObject();
        }

        if (collision.CompareTag("Player") && AIState == AnglerFishState.Wander)
        {
            AIState = AnglerFishState.Attack;
            AgentTarget = collision.GameObject();
        }

        if (collision.CompareTag("Bubble"))
        {
            Destroy(collision.gameObject);
            Health--;
            if (Health <= 0)
            {
                AIState = AnglerFishState.Disoriented;
            }
        }
    }
}

