using UnityEngine;

public class DifficultyNPCManager : MonoBehaviour
{
    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Header("Difficulty")]
    public GameDifficulty difficulty = GameDifficulty.Easy;

    [Header("Always Active - Easy")]
    public GameObject[] easyEnemies;

    [Header("Additional Enemies - Normal")]
    public GameObject[] normalExtraEnemies;

    [Header("Additional Enemies - Hard")]
    public GameObject[] hardExtraEnemies;

    [Header("Result")]
    public int activeEnemyCount;

    void Start()
    {
        ApplyDifficulty();
    }

    public void ApplyDifficulty()
    {
        // Easy用NPCは常に有効
        SetEnemyGroupActive(easyEnemies, true);

        // Normal以上で追加
        bool enableNormal =
            difficulty == GameDifficulty.Normal ||
            difficulty == GameDifficulty.Hard;

        SetEnemyGroupActive(normalExtraEnemies, enableNormal);

        // Hardだけ追加
        bool enableHard =
            difficulty == GameDifficulty.Hard;

        SetEnemyGroupActive(hardExtraEnemies, enableHard);

        activeEnemyCount = CountActiveEnemies();

        Debug.Log(
            "難易度：" + difficulty +
            " / 有効なNPC数：" + activeEnemyCount
        );
    }

    private void SetEnemyGroupActive(GameObject[] enemies, bool active)
    {
        if (enemies == null)
        {
            return;
        }

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(active);
            }
        }
    }

    private int CountActiveEnemies()
    {
        int count = 0;

        count += CountActiveInGroup(easyEnemies);
        count += CountActiveInGroup(normalExtraEnemies);
        count += CountActiveInGroup(hardExtraEnemies);

        return count;
    }

    private int CountActiveInGroup(GameObject[] enemies)
    {
        if (enemies == null)
        {
            return 0;
        }

        int count = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeSelf)
            {
                count++;
            }
        }

        return count;
    }
}