using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private readonly string fileName = "game_state";
    private readonly float updateInterval = 1.0f;
    private GameStateMessage currentState;
    private float lastUpdateTime = 0f;
    private GameObject arena;
    
    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;
    private readonly Dictionary<string, PlayerController> players = new();

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null) return;
        currentState = JsonUtility.FromJson<GameStateMessage>(jsonFile.text);
        if (currentState == null || currentState.players == null) return;
        SpawnPlayers();
    }

    /*void Update()
    {
        lastUpdateTime += Time.deltaTime;
        if (lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = 0f;
            UpdateGameState();
        }
    }*/

    public void SetArena(GameObject arena)
    {
        if (this.arena != null) return;
        this.arena = arena;
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
        Debug.Log(player.username + " - " + player.avatar + "\n");
        var prefab = GetPlayerPrefab(player.avatar);
        if (prefab == null) return;

        GameObject playerObj = Instantiate(prefab, position, rotation);
        playerObj.transform.SetParent(arena.transform);
        playerObj.transform.localScale = new Vector3(1f, 1f, 1f);

        PlayerController playerController = playerObj.AddComponent<PlayerController>();
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
        playerController.Highlight(player.id == currentState.currentTurnPlayerId);

        if (player.state == "attacking" && !string.IsNullOrEmpty(player.attackType))
        {
            playerController.Attack(player.attackType);
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

    private GameObject GetPlayerPrefab(string modelName)
    {
        var avatars = Resources.LoadAll<GameObject>($"Avatars");
        foreach (var prefab in avatars)
        {
            if (prefab.name == modelName)
            {
                Debug.Log($"Found avatar prefab: {prefab.name}");
                return prefab;
            }
        }
        return null;
    }
}

[Serializable]
public class PlayerState
{
    public string id;
    public string username;
    public string avatar;
    public int health;
    public string state;
    public string attackType = "";
    public int attackDamage = 0;
}

[Serializable]
public class GameStateMessage
{
    public PlayerState[] players;
    public string gameStatus;
    public string currentTurnPlayerId;
}