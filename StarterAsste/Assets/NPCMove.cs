using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{
    // 巡回ポイント
    public Transform pointA;
    public Transform pointB;

    private NavMeshAgent agent;

    // 現在の目的地
    private Transform target;

    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 最初の目的地
        target = pointA;

        agent.SetDestination(target.position);

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 目的地に到着したら
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            ChangeTarget();
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    void ChangeTarget()
    {
        // A⇔B切り替え
        if (target == pointA)
        {
            target = pointB;
        }
        else
        {
            target = pointA;
        }

        // 新しい目的地
        agent.SetDestination(target.position);
    }

    //追加
    void OnFootstep()
    {

    }
}