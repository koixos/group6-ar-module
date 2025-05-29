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

    public bool SetArena(GameObject arena)
    {
        if (arena == null || this.arena != null)
            return false;
        this.arena = arena;
        return true;
    }

    public void ProcessGameState(GameState state, bool isFirst)
    {
        if (state.status == "finished" || state == null || state.players == null || state.players.Length == 0)
            return;

        string stateHash = state.currentTurnPlayerId + "_" + string.Join(",", state.players.Select(p => $"{p.id}_{p.health}_{p.state}"));
        if (stateHash == latestState) return;
        latestState = stateHash;
        Debug.Log($"Processing game state: {stateHash}");

        if (isFirst)
            if (!SpawnPlayers(state.players)) return;
            
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

    private bool SpawnPlayers(PlayerStatus[] players)
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

        if (!SpawnPlayer(players[0], p1Pos, p1Rot)) return false;
        if (!SpawnPlayer(players[1], p2Pos, p2Rot)) return false;
        return true;
    }

    private bool SpawnPlayer(PlayerStatus player, Vector3 position, Quaternion rotation)
    {
        var prefab = Resources.Load<GameObject>($"Avatars/{player.avatar}");
        if (prefab == null) return false;
        GameObject playerObj = Instantiate(prefab, position, rotation, arena.transform);
        playerObj.transform.localScale = Vector3.one;

        if (!prefab.TryGetComponent<PlayerController>(out var playerController))
            return false;

        playerController.Initialize(player.id, player.username, player.avatar, player.health, player.maxhealth);
        players.Add(player.id, playerController);
        return true;
    }
}
