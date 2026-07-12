using UnityEngine;
using StarterAssets;

public class PlayerHealth : MonoBehaviour
{
    // 最大HP
    public int maxHealth = 100;

    // 現在HP
    private int currentHealth;

    // Animator
    private Animator animator;

    // 死亡済みかどうか
    public bool isDead = false;

    // 死亡アニメを見せてからゲームオーバーにするまでの時間
    public float gameOverDelay = 3f;

    private ThirdPersonController thirdPersonController;
    private CharacterController characterController;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        characterController = GetComponent<CharacterController>();

        Debug.Log("プレイヤーHP : " + currentHealth);
    }

    // ダメージ処理
    public void TakeDamage(int damage)
    {
        // すでに死亡していたら何もしない
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        Debug.Log("ダメージ！ 現在HP : " + currentHealth);

        // HPが0以下になったら死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        // 被弾アニメーション再生
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("プレイヤー死亡");

        // 操作だけ止める
        // Animatorは止めない。止めると死亡アニメも止まる
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        // CharacterControllerは最初は止めない方が安全
        // 死亡アニメの姿勢が崩れる場合があるため
        // 必要になったら後で有効化する
        /*
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        */

        // 死亡アニメーション再生
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // ここではまだGameOverしない
        // 死亡アニメを見せてからGameOver
        Invoke(nameof(GameOverAfterDelay), gameOverDelay);
    }

    void GameOverAfterDelay()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.GameOver();
        }

        // ここでも最初はDestroyしない方が安全
        // 画面遷移や暗転を確認してから必要なら追加する
        // Destroy(gameObject);
    }
}