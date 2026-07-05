using UnityEngine;

public class EnemyTakeDown : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Animator")]
    public Animator playerAnimator;

    [Header("Enemy Animator")]
    public Animator enemyAnimator;

    [Header("TakeDown Settings")]
    public float takeDownDistance = 2f;

    [Range(-1f, 1f)]
    public float backAngle = -0.5f;

    public KeyCode takeDownKey = KeyCode.E;


    void Update()
    {
        if (Input.GetKeyDown(takeDownKey))
        {
            TryTakeDown();
        }
    }

    private bool isTakeDowned = false;

    void TryTakeDown()
    {
        if (isTakeDowned)
        {
            return;
        }

        EnemyChase chase = GetComponent<EnemyChase>();

        if (chase != null && chase.isChasing)
        {
            Debug.Log("追跡中はテイクダウンできない");
            return;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > takeDownDistance)
        {
            return;
        }

        float dot = Vector3.Dot(transform.forward, directionToPlayer);

        if (dot > backAngle)
        {
            return;
        }

        Debug.Log("テイクダウン成功");

        isTakeDowned = true;

        // プレイヤーアニメ
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("TakeDown");
        }

        // 敵視界停止
        EnemyVision vision = GetComponent<EnemyVision>();

        if (vision != null)
        {
            vision.enabled = false;
        }

        // 追跡停止
        if (chase != null)
        {
            chase.enabled = false;
        }

        // 接触判定も停止
        EnemyTouchDetection touch = GetComponentInChildren<EnemyTouchDetection>();

        if (touch != null)
        {
            touch.enabled = false;
        }

        // NavMesh停止
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true;
        }

        // 死亡アニメ
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetTrigger("Die");
        }
        else
        {
            Debug.Log("enemyAnimatorが入っていない");
        }

        // 数秒後削除
        Destroy(gameObject, 3f);
    }

    void Awake()
    {
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
    }
}