using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;
    //[SerializeField] private GameObject highlight;

    private readonly Dictionary<string, PlayerController> players = new();
    private GameObject arena;
    private string latestState = "";
    private bool isPlayersSpawned = false;

    public void SetArena(GameObject arena)
    {
        if (arena == null || this.arena != null) return;
        this.arena = arena;
    }

    public void ProcessGameState(GameState state)
    {
        if (state.status == "finished" || state == null || state.players == null || state.players.Length == 0)
            return;

        string stateHash = state.currentTurnPlayerId + "_" + string.Join(",", state.players.Select(p => $"{p.id}_{p.health}_{p.state}"));
        if (stateHash == latestState) return;
        latestState = stateHash;
        Debug.Log($"Processing game state: {stateHash}");

        if (!isPlayersSpawned && arena != null)
            SpawnPlayers(state.players);
            
        foreach (var player in state.players)
        {
            if (players.TryGetValue(player.id, out var playerController))
            {
                switch (player.state)
                {
                    case "attack":
                        playerController.Attack(player.attackType);
                        break;
                    case "hurt":
                        playerController.Hurt(player.attackDamage);
                        break;
                    case "victory":
                        playerController.PlayVictoryAnimation();
                        break;
                    case "defeat":
                        playerController.PlayDefeatAnimation();
                        break;
                    default:
                        playerController.PlayIdleAnimation();
                        break;
                }
            }
        }
    }

    private void SpawnPlayers(PlayerStatus[] players)
    {
        Transform arenaTransform = arena.transform;
        Vector3 arenaRight = arenaTransform.right;

        float offsetFromCenter = 6f;
        float topY = (arena.TryGetComponent<Renderer>(out var r)) ? r.bounds.max.y + 0.01f: 0.01f;

        Vector3 p1Pos = arenaTransform.position + arenaRight * offsetFromCenter;
        Vector3 p2Pos = arenaTransform.position - arenaRight * offsetFromCenter;

        p1Pos.y = topY;
        p2Pos.y = topY;

        Quaternion p1Rot = Quaternion.LookRotation(-arenaRight, arenaTransform.up);
        Quaternion p2Rot = Quaternion.LookRotation(arenaRight, arenaTransform.up);

        if (!SpawnPlayer(players[0], p1Pos, p1Rot)) return;
        if (!SpawnPlayer(players[1], p2Pos, p2Rot)) return;

        isPlayersSpawned = true;
    }

    private bool SpawnPlayer(PlayerStatus player, Vector3 position, Quaternion rotation)
    {
        var prefab = GetPlayerPrefab(player.avatar);
        if (prefab == null) return false;
        
        Debug.Log($"Spawning player {player.username} with avatar {player.avatar}");

        GameObject playerObj = Instantiate(prefab, position, rotation, arena.transform);
        playerObj.transform.localScale = Vector3.one;

        var playerController = playerObj.GetComponent<PlayerController>() ?? playerObj.AddComponent<PlayerController>();

        playerController.Initialize(player.id, player.username, player.avatar, player.health, player.maxhealth);

        players.Add(player.id, playerController);
        return true;
    }

    private GameObject GetPlayerPrefab(string name)
    {
        return Resources.LoadAll<GameObject>($"Avatars").FirstOrDefault(a => a.name == name);
    }

    

    /*private void Highlight(bool enable)
    {
        if (highlight == null) return;
        highlight.SetActive(enable);
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
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        else if (newStatus == "ongoing")
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
        else
            foreach (var player in players.Values)
                player.PlayIdleAnimation();
    }*/
}
