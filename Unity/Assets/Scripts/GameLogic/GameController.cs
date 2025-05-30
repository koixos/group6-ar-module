using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

public class GameController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameStatePollingInterval = 2f;  
    [SerializeField] private float attackAnimationDuration = 1f;   
    [SerializeField] private float hurtAnimationDuration = 0.5f;  

    [SerializeField] private GameObject[] attackPrefabs;
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private GameObject damageManagerPrefab;

    private readonly Dictionary<string, PlayerController> players = new();
    private GameObject arena;
    private string latestState = "";

    private bool isProcessingAction = false;
    private Queue<GameAction> actionQueue = new Queue<GameAction>();

    private void Start()
    {
        //StartCoroutine(GameStatePollingRoutine());
        if (SimpleDamageManager.Instance == null)
        {
            GameObject obj = Instantiate(damageManagerPrefab);
            DontDestroyOnLoad(obj); // Sahne geçiþinde silinmesin
            Debug.Log("SimpleDamageManager instantiated.");
        }
    }

    /*private IEnumerator GameStatePollingRoutine()
    {
        bool isFirstState = true;
        while (true)
        {
            yield return new WaitForSeconds(gameStatePollingInterval);
            
            if (arena == null) continue; // Skip if arena is not placed yet

            // Get the current game state from APIManager
            if (APIManager.Instance != null)
            {
                var gameState = APIManager.Instance.GetCurrentGameState();
                if (gameState != null)
                {
                    ProcessGameState(gameState, isFirstState);
                    isFirstState = false;
                }
            }
        }
    }*/

    public bool SetArena(GameObject arena)
    {
        if (arena == null || this.arena != null)
            return false;
        this.arena = arena;
        return true;
    }

    public void ProcessGameState(GameState state, bool isFirst)
    {
        if (state == null || state.players == null || state.players.Length == 0)
        {
            Debug.LogWarning("Invalid game state received");
            return;
        }

        if (state.status == "finished")
        {
            HandleGameEnd(state);
            return;
        }

        string stateHash = state.currentTurnPlayerId + "_" + string.Join(",", state.players.Select(p => $"{p.id}_{p.health}_{p.state}"));
        if (stateHash == latestState) return;
        latestState = stateHash;

        Debug.Log($"Processing game state: {stateHash}");

        if (isFirst)
            if (!SpawnPlayers(state.players)) return;

        // Queue up actions from the state
        foreach (var player in state.players)
        {
            if (players.TryGetValue(player.id, out var playerController))
            {
                QueuePlayerAction(new GameAction
                {
                    PlayerId = player.id,
                    State = player.state,
                    AttackType = player.attackType,
                    Damage = player.attackDamage,
                    Health = player.health
                });
            }
        }

        if (!isProcessingAction)
            StartCoroutine(ProcessActionQueue());
    }

    private void QueuePlayerAction(GameAction action)
    {
        actionQueue.Enqueue(action);
    }

    private IEnumerator ProcessActionQueue()
    {
        isProcessingAction = true;

        while (actionQueue.Count > 0)
        {
            var action = actionQueue.Dequeue();
            
            if (players.TryGetValue(action.PlayerId, out var playerController))
            {
                if (action.Health != playerController.CurrentHealth)
                    playerController.UpdateHealth(action.Health);

                switch (action.State)
                {
                    case "attack":
                        playerController.Attack(action.AttackType);
                        yield return new WaitForSeconds(attackAnimationDuration);
                        break;

                    case "hurt":
                        playerController.Hurt(action.Damage);
                        yield return new WaitForSeconds(hurtAnimationDuration);
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

                yield return new WaitForSeconds(0.2f);
            }
        }

        isProcessingAction = false;
    }

    private void HandleGameEnd(GameState state)
    {
        Debug.Log("Game finished!");
        // Handle game end logic here
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
        Debug.Log($"Attempting to spawn player: {player.username} ({player.avatar})");
        
        if (string.IsNullOrEmpty(player.avatar))
        {
            Debug.LogError($"Avatar name is empty for player {player.username}");
            return false;
        }

        var prefab = Resources.Load<GameObject>($"Avatars/{player.avatar}");
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: Avatars/{player.avatar}");
            return false;
        }

        try
        {
            GameObject playerObj = Instantiate(prefab, position, rotation, arena.transform);
            playerObj.transform.localScale = Vector3.one;
            Debug.Log($"Player object instantiated: {playerObj.name} at position {position}");

            if (!playerObj.TryGetComponent<PlayerController>(out var playerController))
            {
                Debug.LogError($"PlayerController component not found on {playerObj.name}");
                Destroy(playerObj);
                return false;
            }

            playerController.Initialize(player.id, player.username, player.avatar, player.health, player.maxhealth);
            players.Add(player.id, playerController);
            Debug.Log($"Player {player.username} successfully spawned and initialized");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error spawning player {player.username}: {ex.Message}");
            return false;
        }
    }
}

public class GameAction
{
    public string PlayerId;
    public string State;
    public string AttackType;
    public int Damage;
    public int Health;
}
