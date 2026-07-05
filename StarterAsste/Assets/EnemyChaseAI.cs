using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

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
    public Transform pointA;
    public Transform pointB;

    [Header("Speed Settings")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;

    private NavMeshAgent agent;

    public bool isChasing = false;
    private Transform currentTarget;

    [Header("Lose Sight Settings")]
    public float loseSightTime = 2f;

    private float loseTimer = 0f;

    [Header("Suspicious Settings")]
    public float suspiciousTime = 3f;

    public bool isSuspicious = false;

    private float suspiciousTimer = 0f;

    [Header("Look Around Settings")]
    public float rotateSpeed = 60f;
    public float rotateAngle = 45f;

    private float currentRotation = 0f;
    private bool rotateRight = true;

    private Animator animator;
    [HideInInspector]
    public float currentViewDistance;

    [HideInInspector]
    public float currentViewAngle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();

        currentTarget = pointA;
        agent.SetDestination(currentTarget.position);

        agent.speed = patrolSpeed;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        currentViewDistance = normalViewDistance;
        currentViewAngle = normalViewAngle;
    }

    void Update()
    {
        // プレイヤーチェック
        CheckPlayer();

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

        // 歩行判定
        animator.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
    }

    void CheckPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // プレイヤーを視認した
        if (distanceToPlayer < currentViewDistance && angle < currentViewAngle / 2f)
        {
            RaycastHit hit;

            Vector3 rayStart = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(rayStart, directionToPlayer, out hit, currentViewDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isChasing = true;

                    // 警戒解除
                    isSuspicious = false;

                    // 見失いタイマー初期化
                    loseTimer = 0f;

                    return;
                }
            }
        }

        // プレイヤーを見失った時だけタイマー増加
        if (isChasing)
        {
            loseTimer += Time.deltaTime;

            if (loseTimer >= loseSightTime)
            {
                if (!isSuspicious)
                {
                    isChasing = false;

                    isSuspicious = true;

                    suspiciousTimer = 0f;
                }
            }
        }
    }

    void ChasePlayer()
    {
        agent.isStopped = false;

        agent.speed = chaseSpeed;

        // プレイヤーとの距離
        float stopDistance = 1.5f;

        // プレイヤー方向
        Vector3 direction = (transform.position - player.position).normalized;

        // 少し手前を目的地にする
        Vector3 targetPosition = player.position + direction * stopDistance;

        agent.SetDestination(targetPosition);
    }

    void Patrol()
    {
        agent.isStopped = false;

        agent.speed = patrolSpeed;

        if (agent.remainingDistance < 0.5f)
        {
            if (currentTarget == pointA)
            {
                currentTarget = pointB;
            }
            else
            {
                currentTarget = pointA;
            }

            agent.SetDestination(currentTarget.position);
        }
    }

    void Suspicious()
    {
        // 停止
        agent.isStopped = true;

        suspiciousTimer += Time.deltaTime;

        // 左右を見る処理
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

        // 警戒終了
        if (suspiciousTimer >= suspiciousTime)
        {
            isSuspicious = false;

            // リセット
            currentRotation = 0f;

            // 再移動開始
            agent.isStopped = false;

            // 巡回再開
            agent.SetDestination(currentTarget.position);
        }
    }
}