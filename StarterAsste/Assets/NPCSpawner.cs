using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefab")]
    public GameObject npcPrefab;

    [Header("Player")]
    public Transform player;
    public Animator playerAnimator;

    [Header("Routes")]
    public PatrolRoute[] routes;

    void Start()
    {
        SpawnAllNPCs();
    }

    void SpawnAllNPCs()
    {
        foreach (PatrolRoute route in routes)
        {
            SpawnNPC(route);
        }
    }

    void SpawnNPC(PatrolRoute route)
    {
        if (npcPrefab == null)
        {
            Debug.LogError("npcPrefab Ç™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ");
            return;
        }

        if (route == null || route.spawnPoint == null)
        {
            Debug.LogError("Route Ç‹ÇΩÇÕ SpawnPoint Ç™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ");
            return;
        }

        GameObject npc = Instantiate(
            npcPrefab,
            route.spawnPoint.position,
            route.spawnPoint.rotation
        );

        npc.name = "NPC_" + route.name;

        EnemyChase chase = npc.GetComponent<EnemyChase>();

        if (chase != null)
        {
            chase.player = player;
            chase.patrolPoints = route.patrolPoints;
        }

        EnemyVision vision = npc.GetComponent<EnemyVision>();

        if (vision != null)
        {
            vision.player = player;
        }

        EnemyTakeDown takeDown = npc.GetComponent<EnemyTakeDown>();

        if (takeDown != null)
        {
            takeDown.player = player;
            takeDown.playerAnimator = playerAnimator;
            takeDown.enemyAnimator = npc.GetComponent<Animator>();
        }
    }
}