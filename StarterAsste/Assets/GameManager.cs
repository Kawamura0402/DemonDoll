using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverText;
    public GameObject gameClearText;

    // ゲームオーバー
    public void GameOver()
    {
        gameOverText.SetActive(true);

        Invoke(nameof(RestartGame), 3f);
    }

    // ゲームクリア
    public void GameClear()
    {
        gameClearText.SetActive(true);

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
        SceneManager.LoadScene("Title");
    }
}