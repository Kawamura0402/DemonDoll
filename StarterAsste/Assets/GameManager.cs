using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class GameManager : MonoBehaviour
{
    [Header("Game Start Settings")]
    public float preparationTime = 5f;
    public float startTextDisplayTime = 1.5f;
    public GameObject startText;

    public bool IsPlaying { get; private set; } = false;

    private ThirdPersonController playerController;

    public GameObject gameOverText;
    public GameObject gameClearText;

    [Header("Enemy Clear Settings")]
    public bool clearWhenAllEnemiesDefeated = true;

    private int totalEnemyCount = 0;
    private int defeatedEnemyCount = 0;

    private bool isGameEnded = false;

    void Start()
    {
        CountEnemiesInScene();

        playerController = FindFirstObjectByType<ThirdPersonController>();

        StartCoroutine(BeginGameSequence());
    }

    IEnumerator BeginGameSequence()
    {
        // 準備状態
        IsPlaying = false;

        // プレイヤーを停止
        if (playerController != null)
        {
            playerController.SetMovementLocked(true);
        }
        else
        {
            Debug.LogWarning("ThirdPersonControllerが見つかりません");
        }

        // 最初はSTARTを非表示
        if (startText != null)
        {
            startText.SetActive(false);
        }

        Debug.Log("NPC先行時間開始");

        // NPCだけを先に動かす
        yield return new WaitForSeconds(preparationTime);

        // ゲーム開始
        IsPlaying = true;

        if (playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        Debug.Log("ゲームスタート");

        // START表示
        if (startText != null)
        {
            startText.SetActive(true);

            yield return new WaitForSeconds(startTextDisplayTime);

            startText.SetActive(false);
        }
    }

    void CountEnemiesInScene()
    {
        EnemyTakeDown[] enemies = FindObjectsByType<EnemyTakeDown>(FindObjectsSortMode.None);

        totalEnemyCount = enemies.Length;
        defeatedEnemyCount = 0;

        Debug.Log("クリア対象NPC数 : " + totalEnemyCount);
    }

    // 将来NPCSpawnerで生成したNPCを登録する用
    public void RegisterEnemy()
    {
        totalEnemyCount++;

        Debug.Log("NPC登録 : " + defeatedEnemyCount + " / " + totalEnemyCount);
    }

    // EnemyTakeDownから呼ぶ
    public void EnemyDefeated()
    {
        if (isGameEnded)
        {
            return;
        }

        defeatedEnemyCount++;

        Debug.Log("NPC撃破 : " + defeatedEnemyCount + " / " + totalEnemyCount);

        if (clearWhenAllEnemiesDefeated && defeatedEnemyCount >= totalEnemyCount)
        {
            GameClear();
        }
    }

    // ゲームオーバー
    public void GameOver()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        Debug.Log("ゲームオーバー");

        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }

        Invoke(nameof(RestartGame), 3f);
    }

    // ゲームクリア
    public void GameClear()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        Debug.Log("ゲームクリア！ 全NPCを撃破しました");

        if (gameClearText != null)
        {
            gameClearText.SetActive(true);
        }

        Invoke(nameof(ReturnTitle), 3f);
    }

    // リスタート
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // タイトルへ戻る
    void ReturnTitle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}