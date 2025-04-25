using System;
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
    public string state;
    public string attackName = "";
    public int attackDamage = 0;
}

[Serializable]
public class GameStateMessage
{
    public PlayerState[] players;
    public string gameStatus;
    public string turnPlayerId;
}

public class GameController : MonoBehaviour
{
    private readonly string fileName = "game_state";
    private readonly float updateInterval = 1.0f;
    private GameStateMessage currentState;
    private float lastUpdateTime = 0f;

    [SerializeField] private GameObject arena;
    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;
    private readonly Dictionary<string, PlayerController> players = new();

    void Start()
    {
        LoadInitialGameState();
    }

    void Update()
    {
        lastUpdateTime += Time.deltaTime;
        if (lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = 0f;
            UpdateGameState();
        }
    }

    public void SetArena(GameObject arena)
    {
        if (arena != null)
            this.arena = arena;
    }

    private void LoadInitialGameState()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null) return;
        currentState = JsonUtility.FromJson<GameStateMessage>(jsonFile.text);
        if (currentState == null || currentState.players == null) return;
        SpawnPlayers();
    }

    private void UpdateGameState()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null) return;
        GameStateMessage newState = JsonUtility.FromJson<GameStateMessage>(jsonFile.text);
        ProcessGameStateUpdate(newState);
    }

    private void ProcessGameStateUpdate(GameStateMessage newState)
    {
        if (newState == null || newState.players == null) return;

        if (newState.gameStatus != currentState.gameStatus)
            HandleGameStatusChange(newState.gameStatus);

        currentState = newState;

        foreach (var player in newState.players)
            if (players.TryGetValue(player.id, out var playerController))
                HandlePlayerStatusChanged(player, playerController);

        CheckGameEnd(newState);
    }

    private void SpawnPlayers()
    {
        if (arena == null)
            arena = GameObject.FindGameObjectWithTag("GameArena");

        if (arena == null) return;

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

        SpawnPlayer(currentState.players[0], p1Pos, p1Rot);
        SpawnPlayer(currentState.players[1], p2Pos, p2Rot);
    }

    private void SpawnPlayer(PlayerState player, Vector3 position, Quaternion rotation)
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

        HandlePlayerStatusChanged(player, playerController);
    }

    private void CheckGameEnd(GameStateMessage newState)
    {
        foreach (var player in newState.players)
        {
            if (player.health <= 0)
            {
                string winnerId = newState.players.FirstOrDefault(p => p.id != player.id)?.id;
                EndGame(winnerId);
                break;
            }
        }
    }

    private void EndGame(string winnerId)
    {
        if (string.IsNullOrEmpty(winnerId)) return;
        
        foreach (var player in players.Values)
        {
            if (player.id == winnerId)
                player.PlayVictoryAnimation();
            else
                player.PlayDefeatAnimation();
        }
    }

    private void HandleGameStatusChange(string newStatus)
    {
        if (newStatus == "finished")
        {
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        }
        else if (newStatus == "ongoing")
        {
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        }
        else
        {
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        }
    }

    private void HandlePlayerStatusChanged(PlayerState player, PlayerController playerController)
    {
        playerController.Highlight(player.id == currentState.turnPlayerId);

        if (player.state == "attacking" && !string.IsNullOrEmpty(player.attackName))
        {
            playerController.Attack(player.attackName);
            string targetId = currentState.players.FirstOrDefault(p => p.id != player.id)?.id;
            if (!string.IsNullOrEmpty(targetId) && players.TryGetValue(targetId, out var targetPlayer))
                targetPlayer.TakeDamage(player.attackDamage);
        }
        else if (player.state == "victory")
        {
            playerController.PlayVictoryAnimation();
        }
        else if (player.state == "defeat")
        {
            playerController.PlayDefeatAnimation();
        }
        else
        {
            playerController.PlayIdleAnimation();
        }
    }

    private GameObject GetPlayerPrefab(string avatar)
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0) return null;
        foreach (var prefab in playerPrefabs)
            if (prefab != null && prefab.name.Equals(avatar, StringComparison.OrdinalIgnoreCase))
                return prefab;
        return playerPrefabs[0];
    }
}
