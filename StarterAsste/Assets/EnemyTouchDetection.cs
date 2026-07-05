using UnityEngine;

public class EnemyTouchDetection : MonoBehaviour
{
    private EnemyChase enemyChase;

    void Start()
    {
        enemyChase = GetComponentInParent<EnemyChase>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (enemyChase == null || enemyChase.enabled == false)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("プレイヤー接触！");

            enemyChase.isChasing = true;
            enemyChase.isSuspicious = false;
        }
    }
}