using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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