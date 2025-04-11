using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private GameObject arena;

    public void SpawnPlayers()
    {
        if (arena == null)
            arena = GameObject.FindGameObjectWithTag("GameArena");

        GameObject arenaFloor = arena.transform.Find("ArenaFloor").gameObject;
        if (arenaFloor != null)
        {
            Bounds arenaBounds = arenaFloor.GetComponent<Renderer>().bounds;
            Debug.Log($"Arena Bounds - Min: {arenaBounds.min}, Max: {arenaBounds.max}, Center: {arenaBounds.center}");

            /*Vector3 leftCenter = new(arenaBounds.min.x, arenaBounds.center.y, arenaBounds.center.z);
            Vector3 rightCenter = new(arenaBounds.max.x, arenaBounds.center.y, arenaBounds.center.z);
            Debug.Log($"Left Center: {leftCenter}, Right Center: {rightCenter}");*/
            Vector3 leftCenter = new Vector3(-1f, 0.5f, 0f);
            Vector3 rightCenter = new Vector3(1f, 0.5f, 0f);

            SpawnPlayer(player1Prefab, leftCenter, Quaternion.Euler(0, 90, 0));
            SpawnPlayer(player2Prefab, rightCenter, Quaternion.Euler(0, -90, 0));
        }
    }

    private void SpawnPlayer(GameObject playerPrefab, Vector3 position, Quaternion rotation)
    {
        if (playerPrefab == null) return;
        GameObject player = Instantiate(playerPrefab, position, rotation);
        player.transform.localScale = new Vector3(1f, 1f, 1f);
        Debug.Log($"Player spawned at position: {position}");
    }
}
