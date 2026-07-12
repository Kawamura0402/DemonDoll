using UnityEngine;
using StarterAssets;

public class EnemyVision : MonoBehaviour
{
    // 視界距離
    public float currentDistance = 10f;

    // 視野角
    public float currentAngle = 60f;

    // ダメージ量
    public int damage = 100;

    // ダメージ間隔
    public float damageInterval = 1.5f;

    // プレイヤー
    public Transform player;

    private float timer = 0f;

    private EnemyChase chaseAI;

    private ThirdPersonController playerController;

    [Header("Crouch Detection")]
    [Range(0.1f, 1f)]
    public float crouchViewDistanceMultiplier = 0.55f;

    void Start()
    {
        chaseAI = GetComponent<EnemyChase>();

        if (player != null)
        {
            playerController = player.GetComponent<ThirdPersonController>();
        }
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        if (playerHealth.isDead)
        {
            return;
        }

        if (PlayerInSight())
        {
            timer += Time.deltaTime;

            if (timer >= damageInterval)
            {
                Debug.Log("プレイヤー発見");

                playerHealth.TakeDamage(damage);

                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    bool PlayerInSight()
    {
        if (player == null)
        {
            return false;
        }

        // 敵の目の高さ
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;

        // プレイヤー中心
        Vector3 playerPosition = player.position + Vector3.up * 1f;

        // プレイヤー方向
        Vector3 directionToPlayer = (playerPosition - eyePosition).normalized;

        // 距離
        float distance = Vector3.Distance(eyePosition, playerPosition);

        float effectiveDistance = currentDistance;

        if (chaseAI != null)
        {
            effectiveDistance = chaseAI.currentViewDistance;
        }

        if (chaseAI != null &&
            !chaseAI.isChasing &&
            playerController != null &&
            playerController.IsCrouching)
        {
            effectiveDistance *= crouchViewDistanceMultiplier;
        }

        // 距離チェック
        if (distance > effectiveDistance)
        {
            return false;
        }


        // 角度チェック
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > currentAngle / 2)
        {
            return false;
        }
           

        // Raycast
        RaycastHit hit;

        if (Physics.Raycast(eyePosition, directionToPlayer, out hit, effectiveDistance))
        {

            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        if (player == null)
        {
            return;
        }

        EnemyChase chaseAI = GetComponent<EnemyChase>();

        float currentDistance = this.currentDistance;
        float currentAngle = this.currentAngle;

        if (chaseAI != null)
        {
            currentDistance = chaseAI.currentViewDistance;
            currentAngle = chaseAI.currentViewAngle;
        }

        // 目の位置
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;

        // ===== 扇形の描画 =====

        // 半透明の色
        Color visionColor;

        // 発見中
        if (chaseAI != null && chaseAI.isChasing)
        {
            visionColor = new Color(1f, 0f, 0f, 0.25f);
        }

        // 警戒中
        else if (chaseAI != null && chaseAI.isSuspicious)
        {
            visionColor = new Color(1f, 0.5f, 0f, 0.25f);
        }

        // 通常
        else
        {
            visionColor = new Color(0f, 1f, 0f, 0.25f);
        }

        Gizmos.color = visionColor;

        // 扇形分割数
        int segments = 30;

        // 開始角度
        float startAngle = -currentAngle / 2f;

        Vector3 previousPoint = eyePosition;

        for (int i = 0; i <= segments; i++)
        {
            // 現在角度


            // 回転方向
            float angle = startAngle + (currentAngle / segments) * i;

            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            // 扇形の点
            Vector3 nextPoint;

            RaycastHit hit;

            // 壁に当たったか
            if (Physics.Raycast(eyePosition, direction, out hit, currentDistance))
            {
                // 壁まで
                nextPoint = hit.point;
            }
            else
            {
                // 最大距離
                nextPoint = eyePosition + direction * currentDistance;
            }

            // 線を描画
            Gizmos.DrawLine(eyePosition, nextPoint);

            // 前の点と接続
            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, nextPoint);
            }

            previousPoint = nextPoint;
        }

        // ===== 境界線 =====

        Gizmos.color = Color.yellow;

        Vector3 leftDirection = Quaternion.Euler(0, -currentAngle / 2, 0) * transform.forward;

        Vector3 rightDirection = Quaternion.Euler(0, currentAngle / 2, 0) * transform.forward;

        Gizmos.DrawRay(eyePosition, leftDirection * currentDistance);
        Gizmos.DrawRay(eyePosition, rightDirection * currentDistance);

        // ===== 正面 =====

        Gizmos.color = Color.white;

        Gizmos.DrawRay(eyePosition, transform.forward * currentDistance);

        // ===== プレイヤー方向 =====

        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - eyePosition).normalized;

            float distance = Vector3.Distance(eyePosition, player.position);

            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (distance <= currentDistance && angle <= currentAngle / 2)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawRay(eyePosition, directionToPlayer * distance);
            }
        }
    }
}


