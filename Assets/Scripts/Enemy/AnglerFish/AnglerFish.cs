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
    public Animator SpriteAnimatior;

    public CircleCollider2D DetectionCollider;
    public int NumberOfAttack = 10;

    IEnumerator Start()
    {

        //Initialize Value
        AIState = AnglerFishState.Idle;

        while (true)
        {
            switch (AIState)
            {
                case AnglerFishState.Idle:
                    break;
                case AnglerFishState.Emerge:
                    SpriteAnimatior.SetBool("IsEmerge", true);

                    yield return new WaitUntil(() =>
                    {
                        AnimatorStateInfo stateInfo = SpriteAnimatior.GetCurrentAnimatorStateInfo(0);
                        return stateInfo.IsName("EmergeMove") && stateInfo.normalizedTime >= 1.0f;
                    });

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
            Debug.Log(AIState);

            yield return 0;
        }
    }

    public void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("collide");
        if (collision.CompareTag("Player")) {
            AIState = AnglerFishState.Emerge;
        }
    }
}

