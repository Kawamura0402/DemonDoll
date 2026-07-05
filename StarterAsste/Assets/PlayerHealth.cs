using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // 最大HP
    public int maxHealth = 100;

    // 現在HP
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        Debug.Log("プレイヤーHP : " + currentHealth);
    }

    // ダメージ処理
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("ダメージ！ 現在HP : " + currentHealth);

        // HPが0以下になったら死亡
        if (currentHealth <= 0)
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.GameOver();
            }

            Die();
        }
    }

    // ゲームオーバー処理
    void Die()
    {
        Debug.Log("ゲームオーバー");

        // プレイヤー削除
        Destroy(gameObject);
    }
}