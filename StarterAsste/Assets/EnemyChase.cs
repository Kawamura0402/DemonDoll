using UnityEngine;
using UnityEngine.AI;
using StarterAssets;

public class EnemyChase : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Crouch Detection")]
    [Range(0.1f, 1f)]
    public float crouchViewDistanceMultiplier = 0.65f;

    public float minimumCrouchDetectionDistance = 3f;

    private ThirdPersonController playerController;

    [Header("Vision Settings")]
    [Header("Normal Vision")]
    public float normalViewDistance = 6f;
    public float normalViewAngle = 45f;

    [Header("Suspicious Vision")]
    public float suspiciousViewDistance = 10f;
    public float suspiciousViewAngle = 90f;

    [Header("Chase Vision")]
    public float chaseViewDistance = 14f;
    public float chaseViewAngle = 120f;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    [Header("Patrol Settings")]
    public float patrolArriveDistance = 0.6f;
    public float waitTimeAtPoint = 2f;
    public bool lookAroundAtPoint = true;

    [Header("Speed Settings")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;

    private NavMeshAgent agent;
    private Animator animator;

    public bool isChasing = false;
    public bool isSuspicious = false;

    private Transform currentTarget;
    private int currentPatrolIndex = 0;

    [Header("Lose Sight Settings")]
    public float loseSightTime = 2f;
    private float loseTimer = 0f;

    [Header("Suspicious Settings")]
    public float suspiciousTime = 3f;
    private float suspiciousTimer = 0f;

    [Header("Look Around Settings")]
    public float rotateSpeed = 60f;
    public float rotateAngle = 45f;

    private float currentRotation = 0f;
    private bool rotateRight = true;

    private bool isWaitingAtPoint = false;
    private float waitTimer = 0f;

    [HideInInspector]
    public float currentViewDistance;

    [HideInInspector]
    public float currentViewAngle;

    private GameManager gameManager;

    private Vector3 lastSeenPosition;
    private bool hasLastSeenPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        gameManager = FindFirstObjectByType<GameManager>();

        agent.speed = patrolSpeed;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            playerController = player.GetComponent<ThirdPersonController>();
        }

        currentViewDistance = normalViewDistance;
        currentViewAngle = normalViewAngle;

        // 巡回ポイントが設定されている場合、最初のポイントへ移動
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            currentTarget = patrolPoints[currentPatrolIndex];

            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            Debug.LogWarning("PatrolPoints が設定されていません。NPCに巡回ポイントを設定してください。");
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }

            return;
        }

        if (player == null)
        {
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.isDead)
        {
            isChasing = false;
            isSuspicious = false;
            isWaitingAtPoint = false;

            if (agent != null)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }

            animator.SetBool("isWalking", false);

            return;
        }

        // プレイヤーを見つけたか確認
        // ゲーム開始後だけプレイヤーを発見する
        if (gameManager == null || gameManager.IsPlaying)
        {
            CheckPlayer();
        }

        if (isChasing)
        {
            currentViewDistance = chaseViewDistance;
            currentViewAngle = chaseViewAngle;

            ChasePlayer();
        }
        else if (isSuspicious)
        {
            currentViewDistance = suspiciousViewDistance;
            currentViewAngle = suspiciousViewAngle;

            Suspicious();
        }
        else
        {
            currentViewDistance = normalViewDistance;
            currentViewAngle = normalViewAngle;

            Patrol();
        }

        // 歩行アニメーション
        if (animator != null && agent != null)
        {
            animator.SetBool("isWalking", agent.velocity.magnitude > 0.1f && !agent.isStopped);
        }
    }

    void CheckPlayer()
    {
        if (player == null)
        {
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.isDead)
        {
            return;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // プレイヤーが視界内にいるか
        float effectiveViewDistance = GetEffectiveViewDistance();

        if (distanceToPlayer < effectiveViewDistance &&
            angle < currentViewAngle / 2f)
        {
            RaycastHit hit;

            Vector3 rayStart = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(rayStart, directionToPlayer, out hit, effectiveViewDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isChasing = true;
                    isSuspicious = false;
                    isWaitingAtPoint = false;

                    loseTimer = 0f;
                    suspiciousTimer = 0f;
                    waitTimer = 0f;

                    agent.isStopped = false;

                    return;
                }
            }
        }

        // 追跡中にプレイヤーを見失った場合
        if (isChasing)
        {
            loseTimer += Time.deltaTime;

            if (loseTimer >= loseSightTime)
            {
                isChasing = false;
                isSuspicious = true;

                suspiciousTimer = 0f;
                currentRotation = 0f;
                rotateRight = true;
            }
        }
    }

    void ChasePlayer()
    {
        if (agent == null || player == null)
        {
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        float stopDistance = 1.5f;

        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 targetPosition = player.position + direction * stopDistance;

        agent.SetDestination(targetPosition);
    }

    void Patrol()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        agent.speed = patrolSpeed;

        if (isWaitingAtPoint)
        {
            agent.isStopped = true;

            waitTimer += Time.deltaTime;

            if (lookAroundAtPoint)
            {
                LookAround();
            }

            if (waitTimer >= waitTimeAtPoint)
            {
                MoveToNextPatrolPoint();
            }

            return;
        }

        agent.isStopped = false;

        if (currentTarget == null)
        {
            currentTarget = patrolPoints[currentPatrolIndex];
            agent.SetDestination(currentTarget.position);
        }

        if (!agent.pathPending && agent.remainingDistance <= patrolArriveDistance)
        {
            StartWaitingAtPoint();
        }
    }

    void StartWaitingAtPoint()
    {
        isWaitingAtPoint = true;

        waitTimer = 0f;
        currentRotation = 0f;
        rotateRight = true;

        agent.ResetPath();
        agent.isStopped = true;
    }

    void MoveToNextPatrolPoint()
    {
        isWaitingAtPoint = false;

        waitTimer = 0f;
        currentRotation = 0f;
        rotateRight = true;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        currentTarget = patrolPoints[currentPatrolIndex];

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
    }

    void Suspicious()
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = true;

        suspiciousTimer += Time.deltaTime;

        LookAround();

        if (suspiciousTimer >= suspiciousTime)
        {
            isSuspicious = false;

            suspiciousTimer = 0f;
            currentRotation = 0f;
            rotateRight = true;

            agent.isStopped = false;

            // 警戒後は、巡回ルートへ戻る
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.position);
            }
        }
    }

    void LookAround()
    {
        float rotationThisFrame = rotateSpeed * Time.deltaTime;

        if (rotateRight)
        {
            transform.Rotate(0, rotationThisFrame, 0);
            currentRotation += rotationThisFrame;

            if (currentRotation >= rotateAngle)
            {
                rotateRight = false;
            }
        }
        else
        {
            transform.Rotate(0, -rotationThisFrame, 0);
            currentRotation -= rotationThisFrame;

            if (currentRotation <= -rotateAngle)
            {
                rotateRight = true;
            }
        }
    }

    public float GetEffectiveViewDistance()
    {
        float effectiveDistance = currentViewDistance;

        if (!isChasing &&
            playerController != null &&
            playerController.IsCrouching)
        {
            float distanceToPlayer =
                Vector3.Distance(transform.position, player.position);

            // 近距離では、しゃがみの視界距離軽減を使わない
            if (distanceToPlayer > minimumCrouchDetectionDistance)
            {
                effectiveDistance *= crouchViewDistanceMultiplier;
            }
        }

        return effectiveDistance;
    }
}