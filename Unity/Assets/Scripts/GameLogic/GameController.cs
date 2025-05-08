using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{  
    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;
    private readonly Dictionary<string, PlayerController> players = new();
    private GameState currentState;
    private GameObject arena;
    private int turnCounter = 0;

    public void OnWebSocketMsg(string json)
    {
        Debug.Log($"WebSocket message: {json}");
        GameState state = JsonUtility.FromJson<GameState>(json);
        if (state == null || state.players == null || state.players.Length == 0) return;
        currentState = state;
        ++turnCounter;

        if (turnCounter == 1)
            SpawnPlayers();
        
        ProcessGameState();
    }

    public void SetArena(GameObject arena)
    {
        if (this.arena != null) return;
        this.arena = arena;
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

        Debug.Log($"Spawning players at {p1Pos} and {p2Pos} with rotations {p1Rot} and {p2Rot}");

        SpawnPlayer(currentState.players[0], p1Pos, p1Rot);
        SpawnPlayer(currentState.players[1], p2Pos, p2Rot);
    }

    private void SpawnPlayer(PlayerState player, Vector3 position, Quaternion rotation)
    {
        var prefab = GetPlayerPrefab(player.avatar);
        if (prefab == null) return;
        Debug.Log($"Spawning player {player.username} with avatar {player.avatar}");

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
    }

    private void ProcessGameState()
    {
        //HandleGameStateChange();
        //HandlePlayerStateChanges();
    }

    private void HandleGameStateChange()
    {
        if (currentState.gameStatus == "finished")
        {
            // END SCREEN
        }
    }

    private void HandlePlayerStateChanges(PlayerState player, PlayerController playerController)
    {
        playerController.Highlight(player.id == currentState.currentTurnPlayerId);

        if (player.state == "attacking" && !string.IsNullOrEmpty(player.attackType))
        {
            playerController.Attack(player.attackType);
            string targetId = currentState.players.FirstOrDefault(p => p.id != player.id)?.id;
            if (!string.IsNullOrEmpty(targetId) && players.TryGetValue(targetId, out var targetPlayer))
                targetPlayer.TakeDamage(player.attackDamage);
            return;
        }

        if (player.state == "winner")
            playerController.PlayVictoryAnimation();
        else if (player.state == "dead")
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
