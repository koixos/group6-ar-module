using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private enum InitState
    {
        WaitingForArena,
        WaitingForServerData,
        ReadyToSpawn,
        GameActive
    }

    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;

    private readonly Dictionary<string, PlayerController> players = new();
    private InitState currentState = InitState.WaitingForArena;
    private GameState currentGameState;
    private GameObject arena;
    private string lastReceivedJson = "";
    private float dataTimeoutTimer = 0f;
    private const float timeoutThreshold = 10f;
    private bool isPlayerDataReady = false;
    private bool isPlayersSpawned = false;

    void Update()
    {
        switch (currentState)
        {
            case InitState.WaitingForArena:
                if (arena != null)
                    currentState = InitState.WaitingForServerData;
                break;
            case InitState.WaitingForServerData:
                dataTimeoutTimer += Time.deltaTime;
                if (isPlayerDataReady && arena != null)
                {
                    dataTimeoutTimer = 0f;
                    if (!isPlayersSpawned)
                        currentState = InitState.ReadyToSpawn;
                    else
                        currentState = InitState.GameActive;
                }
                else if (dataTimeoutTimer > timeoutThreshold)
                {
                    Debug.LogWarning("Timeout waiting for server data.");
                    return;
                    // Handle timeout (e.g., show error message, retry, etc.)
                }
                break;
            case InitState.ReadyToSpawn:
                if (SpawnPlayers())
                {
                    isPlayersSpawned = true;
                    currentState = InitState.GameActive;
                }
                break;
            case InitState.GameActive:
                ProcessGameState();
                break;
        }
    }

    public void OnWebSocketMsg(string json)
    {
        if (json == lastReceivedJson) return;
        lastReceivedJson = json;

        GameState state = JsonUtility.FromJson<GameState>(json);
        if (state == null || state.players == null || state.players.Length == 0) return;
        isPlayerDataReady = true;
        currentGameState = state;
    }

    public void SetArena(GameObject arena)
    {
        if (this.arena != null) return;
        this.arena = arena;
        if (currentState == InitState.WaitingForArena && isPlayerDataReady)
            currentState = InitState.WaitingForServerData;
    }

    private bool SpawnPlayers()
    {
        if (arena == null) return false;

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

        SpawnPlayer(currentGameState.players[0], p1Pos, p1Rot);
        SpawnPlayer(currentGameState.players[1], p2Pos, p2Rot);

        return true;
    }

    private void SpawnPlayer(PlayerState player, Vector3 position, Quaternion rotation)
    {
        var prefab = GetPlayerPrefab(player.avatar);
        if (prefab == null) return;
        Debug.Log($"Spawning player {player.username} with avatar {player.avatar}");

        GameObject playerObj = Instantiate(prefab, position, rotation);
        playerObj.transform.SetParent(arena.transform);
        playerObj.transform.localScale = new Vector3(1f, 1f, 1f);

        if (!playerObj.TryGetComponent<PlayerController>(out var playerController))
            playerController = playerObj.AddComponent<PlayerController>();

        playerController.Initialize(
            player.id,
            player.username,
            player.avatar,
            player.health
        );

        players.Add(player.id, playerController);
    }

    private void ProcessGameState()
    {
        if (currentGameState.gameStatus == "finished")
        {
            // END SCREEN
        }
        else
        {
            foreach (var player in currentGameState.players)
            {
                if (players.TryGetValue(player.id, out var playerController))
                {
                    //Debug.Log(player.avatar);
                    HandlePlayerStateChange(player, playerController);
                    //playerController.SetHealth(player.health);
                }
            }
        }
    }

    private void HandlePlayerStateChange(PlayerState player, PlayerController playerController)
    {
        //playerController.Highlight(player.id == currentState.currentTurnPlayerId);

        if (player.state == "attack" && !string.IsNullOrEmpty(player.attackType))
        {
            playerController.Attack(player.attackType);
            string targetId = currentGameState.players.FirstOrDefault(p => p.id != player.id)?.id;
            if (!string.IsNullOrEmpty(targetId) && players.TryGetValue(targetId, out var targetPlayer))
                targetPlayer.TakeDamage(player.attackDamage);
            return;
        }

        if (player.state == "victory")
            playerController.PlayVictoryAnimation();
        else if (player.state == "defeat")
            playerController.PlayDefeatAnimation();
        else
            playerController.PlayIdleAnimation();
    }

    /*private void CheckGameEnd(GameStateMessage newState)
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

    /*private void HandleGameStatusChange(string newStatus)
    {
        if (newStatus == "finished")
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        else if (newStatus == "ongoing")
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        else
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
    }*/

    private GameObject GetPlayerPrefab(string modelName)
    {
        var avatars = Resources.LoadAll<GameObject>($"Avatars");
        foreach (var prefab in avatars)
            if (prefab.name == modelName)
                return prefab;
        return null;
    }
}

[Serializable]
public class GameState
{
    public string gameStatus;
    public string currentTurnPlayerId;
    public PlayerState[] players;
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
