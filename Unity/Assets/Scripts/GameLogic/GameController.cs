using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private WebSocketManager wsManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private ARSetup arSetup;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private TMP_InputField roomCodeInp;
    [SerializeField] private GameObject roomCodePanel;
    [SerializeField] private GameObject gameInterface;
    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private Transform arenaTransform;

    private string currTurn = "";
    private int turnCount = 0;
    private bool gameActive = false;    

    private readonly Dictionary<string, PlayerController> players = new();
    private readonly Vector3[] playerPositions = new Vector3[]
    {
        new(-0.5f, 0, 0),
        new(0.5f, 0, 0)
    };

    void Start()
    {
        if (wsManager == null)
            wsManager = FindObjectOfType<WebSocketManager>();

        if (arSetup == null)
            arSetup = FindObjectOfType<ARSetup>();

        ShowRoomCodePanel(true);
        ShowGameInterface(false);

        if (networkManager != null)
            networkManager.OnConnectionStatusChanged += OnConnectionStatusChanged;
    }

    void OnDestroy()
    {
        if (networkManager != null)
            networkManager.OnConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    public void OnARSetupComplete()
    {
        Debug.Log("AR Setup complete, arena placed");
        if (arenaTransform == null)
        {
            GameObject arena = GameObject.FindGameObjectWithTag("GameArena");
            if (arena != null) 
                arenaTransform = arena.transform;
        }

        if (playerSpawner == null)
            playerSpawner = FindObjectOfType<PlayerSpawner>();

        if (playerSpawner != null)
            playerSpawner.SpawnPlayers();
        else
            Debug.LogError("PlayerSpawner not found!");
    }

    public void JoinRoom()
    {
        if (roomCodeInp == null || string.IsNullOrEmpty(roomCodeInp.text))
        {
            Debug.LogError("Room code is empty");
            return;
        }

        string roomCode = roomCodeInp.text.Trim();
        wsManager.JoinRoom(roomCode);

        if (networkManager != null)
            networkManager.SetRoomCode(roomCode);

        ShowRoomCodePanel(false);
        ShowGameInterface(true);
    }

    public void UpdateGameState(WebSocketManager.GameStateMessage gameState)
    {
       if (gameState == null || gameState.players == null)
            return;

        currTurn = gameState.currTurn;
        turnCount = gameState.turnCount;

        if (arenaTransform == null)
        {
            Debug.LogWarning("Arena not ready yet!");
            GameObject arena = GameObject.FindGameObjectWithTag("GameArena");
            if (arena == null)
            {
                Debug.LogError("Cannot find arena transform");
                return;
            }
            arenaTransform = arena.transform;                
        }

        for (int i = 0; i < gameState.players.Length; i++)
        {
            var playerState = gameState.players[i];
            if (string.IsNullOrEmpty(playerState.id))
                continue;

            Vector3 worldPos;
            if (playerState.pos == null)
            {
                int posInd = i % playerPositions.Length;
                worldPos = arenaTransform.TransformPoint(playerPositions[posInd]);
            }
            else
            {
                worldPos = arenaTransform.TransformPoint(playerState.pos.ToVector3());
            }
                
            if (!players.ContainsKey(playerState.id))
            {
                GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);
                if (playerObj.TryGetComponent<PlayerController>(out var playerController))
                {
                    playerController.Initialize(
                        playerState.id,
                        playerState.name,
                        playerState.avatar,
                        playerState.health,
                        worldPos
                    );
                    players[playerState.id] = playerController;
                }
            }
            else
            {
                PlayerController playerController = players[playerState.id];
                playerController.SetModel(playerState.avatar);
                playerController.UpdateHealth(playerState.health);
                StartCoroutine(SmoothMove(playerController.transform, worldPos, 0.5f));
            }
        }

        gameActive = true;

        UpdateTurnIndicator();
    }

    public void HandlePlayerAction(WebSocketManager.PlayerActionMessage action)
    {
        if (!gameActive || action == null || string.IsNullOrEmpty(action.playerId))
            return;

        if (!players.ContainsKey(action.playerId))
        {
            Debug.LogError("Player not found: " + action.playerId);
            return;
        }

        PlayerController player = players[action.playerId];

        switch (action.attackType)
        {
            case "attack":
                HandleAttack(player, action);
                break;
            case "move":
                break;
            default:
                Debug.LogWarning("Unknown action type: " + action.attackType);
                break;
        }
    }

    private void UpdateTurnIndicator()
    {
        // Implementation depends on UI setup
        Debug.Log($"Current turn: {currTurn}, Turn count: {turnCount}");
    }

    private void OnConnectionStatusChanged(bool isConnected)
    {
        if (!isConnected && gameActive)
        {
            Debug.Log("Disconnected from server");
            ShowRoomCodePanel(true);
            ShowGameInterface(false);
        }
    }

    private void HandleAttack(PlayerController attacker, WebSocketManager.PlayerActionMessage action)
    {
        attacker.PlayAttackAnimation(action.attackName);

        if (string.IsNullOrEmpty(action.targetId) || !players.ContainsKey(action.targetId))
        {
            Debug.LogError("Target not found: " + action.targetId);
            return;
        }

        PlayerController target = players[action.targetId];

        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab);
            StartCoroutine(AnimateEffect(effect, attacker.transform.position, target.transform.position));
        }

        StartCoroutine(DelayedAction(0.5f, () =>
        {
            target.TakeDamage(action.damage);
        }));
    }

    private void ShowRoomCodePanel(bool show)
    {
        if (roomCodePanel != null)
            roomCodePanel.SetActive(show);
    }

    private void ShowGameInterface(bool show)
    {
        if (gameInterface != null)
            gameInterface.SetActive(show);
    }

    private IEnumerator AnimateEffect(GameObject effect, Vector3 start, Vector3 end)
    {
        if (effect == null)
            yield break;

        float duration = 0.5f;
        float elapsed = 0f;

        effect.transform.position = start;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            effect.transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        effect.transform.position = end;

        if (effect.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play();
            float destroyDelay = ps.main.duration + ps.main.startLifetimeMultiplier;
            Destroy(effect, destroyDelay);
        }
        else
        {
            Destroy(effect, 0.5f);
        }
    }

    private IEnumerator SmoothMove(Transform objTransform, Vector3 targetPos, float duration)
    {
        Vector3 startPos = objTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            objTransform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        objTransform.position = targetPos;
    }

    private IEnumerator DelayedAction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
