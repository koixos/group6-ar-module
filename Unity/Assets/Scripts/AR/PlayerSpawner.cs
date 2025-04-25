using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    public void SpawnPlayers(GameObject arena)
    {
        if (arena == null)
            arena = GameObject.FindGameObjectWithTag("GameArena");

        Transform arenaTransform = arena.transform;
        Vector3 arenaRight = arenaTransform.right;

        float offsetFromCenter = 6f;
        float topY = 0f;
        if (arena.TryGetComponent<Renderer>(out var arenaRenderer))
            topY = arenaRenderer.bounds.max.y;

        Vector3 p1Pos = arenaTransform.position + arenaRight * offsetFromCenter;
        Vector3 p2Pos = arenaTransform.position - arenaRight * offsetFromCenter;

        p1Pos.y = topY + 0.01f;
        p2Pos.y = topY + 0.01f;

        Quaternion p1Rot = Quaternion.LookRotation(-arenaTransform.right, arenaTransform.up);
        Quaternion p2Rot = Quaternion.LookRotation(arenaTransform.right, arenaTransform.up);

        SpawnPlayer(player1Prefab, p1Pos, p1Rot, arena);
        SpawnPlayer(player2Prefab, p2Pos, p2Rot, arena);
    }

    private void SpawnPlayer(GameObject playerPrefab, Vector3 position, Quaternion rotation, GameObject arena)
    {
        if (playerPrefab == null) return;
        GameObject player = Instantiate(playerPrefab, position, rotation);
        player.transform.SetParent(arena.transform);
        player.transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
