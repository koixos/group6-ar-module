using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerState
{
    public string id;
    public string username;
    public string avatar;
    public int health;
    public string attackName;
    public string state;
    public int attackDamage;
}

[Serializable]
public class GameStateMessage
{
    public string type = "game_state";
    public PlayerState[] players;
    public string gameStatus;
}

public class GameController : MonoBehaviour
{
    private readonly string fileName = "game_state";
    private GameStateMessage currentState;

    [SerializeField] private GameObject arena;
    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;
    private readonly Dictionary<string, PlayerController> players = new();
    
    //private bool gameActive = false;
    //public string gameStatus;   // "idle", "ongoing", "finished"

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null) return;
        currentState = JsonUtility.FromJson<GameStateMessage>(jsonFile.text);
        SpawnPlayers(currentState);
    }

    public void SpawnPlayers(GameStateMessage gameState)
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

        var players = gameState.players;
        SpawnPlayer(players[0], p1Pos, p1Rot, arena);
        SpawnPlayer(players[1], p2Pos, p2Rot, arena);
    }

    public void UpdateGameState(GameStateMessage gameState)
    {
        foreach (var player in gameState.players)
        {    
            if (players.TryGetValue(player.id, out var playerController))
                playerController.SetHealth(player.health);

            if (!string.IsNullOrEmpty(player.attackName) && player.state == "attacking")
            {
                playerController.PlayAttackAnimation(player.attackName);
                string targetId = gameState.players.FirstOrDefault(x => x.id != player.id)?.id;
                if (targetId != null && players.ContainsKey(targetId))
                    players[targetId].TakeDamage(player.attackDamage);
            }
        }
    }

    public void SetArena(GameObject arena)
    {
        if (arena != null)
            this.arena = arena;
    }

    private void SpawnPlayer(PlayerState player, Vector3 position, Quaternion rotation, GameObject arena)
    {
        GameObject playerPrefab = GetPlayerPrefab(player.avatar);
        if (playerPrefab == null) return;
        GameObject playerObj = Instantiate(playerPrefab, position, rotation);
        playerObj.transform.SetParent(arena.transform);
        playerObj.transform.localScale = new Vector3(1f, 1f, 1f);

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.Initialize(
            player.id,
            player.username,
            player.avatar,
            player.health
        );

        players.Add(player.id, playerController);
    }

    private GameObject GetPlayerPrefab(string avatar)
    {
        foreach (var prefab in playerPrefabs)
            if (prefab.name.Equals(avatar, StringComparison.OrdinalIgnoreCase))
                return prefab;
        return null;
    }
}
