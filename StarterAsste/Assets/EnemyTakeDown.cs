using StarterAssets;
using System.Collections;
using UnityEngine;

public class EnemyTakeDown : MonoBehaviour
{
    [Header("TakeDown Timing")]
    public float playerMovementLockTime = 1.1f;

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

    private bool isTakeDowned = false;
    private bool hasReportedDefeated = false;

    private static bool anyTakeDownInProgress = false;

    bool IsAnyEnemyChasing()
    {
        EnemyChase[] enemies =
            Object.FindObjectsByType<EnemyChase>(
                FindObjectsSortMode.None
            );

        foreach (EnemyChase enemy in enemies)
        {
            if (enemy != null && enemy.isChasing)
            {
                return true;
            }
        }

        return false;
    }

    void Awake()
    {
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (anyTakeDownInProgress)
        {
            return;
        }

        if (Input.GetKeyDown(takeDownKey))
        {
            TryTakeDown();
        }
    }

    void TryTakeDown()
    {
        if (anyTakeDownInProgress)
        {
            return;
        }

        if (IsAnyEnemyChasing())
        {
            Debug.Log("いずれかのNPCに発見中のためTakeDownできません");
            return;
        }

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

        if (player == null)
        {
            Debug.LogWarning("Playerが設定されていません");
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

        // 全NPC共通で「テイクダウン中」にする
        anyTakeDownInProgress = true;

        // このNPCを撃破済みにする
        isTakeDowned = true;

        // プレイヤーの移動を停止する
        StartCoroutine(LockPlayerMovement());

        // GameManagerへ撃破報告
        if (!hasReportedDefeated)
        {
            hasReportedDefeated = true;

            GameManager gameManager = FindFirstObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.EnemyDefeated();
            }
            else
            {
                Debug.LogWarning("GameManagerが見つかりません");
            }
        }

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
            agent.ResetPath();
            agent.enabled = false;
        }

        // Collider停止
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();

        if (capsule != null)
        {
            capsule.enabled = false;
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

    IEnumerator LockPlayerMovement()
    {
        ThirdPersonController controller =
            player.GetComponent<ThirdPersonController>();

        if (controller == null)
        {
            anyTakeDownInProgress = false;
            yield break;
        }

        controller.SetMovementLocked(true);

        yield return new WaitForSeconds(playerMovementLockTime);

        if (controller != null)
        {
            controller.SetMovementLocked(false);
        }

        anyTakeDownInProgress = false;
    }
}