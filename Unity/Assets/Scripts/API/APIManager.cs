using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }

    [Header("API Configuration")]
    public string baseUrl = "http://192.168.137.1:3001/api/ar";
    private string sessionId;
    private string authToken;
    private GameState currentGameState;

    private Coroutine joinRoomCoroutine;
    private bool isJoiningRoom = false;

    public event Action<ServerResponse> OnJoinRoomResponse;
    public event Action<ServerResponse> OnGameStateUpdated;
    public event Action<string> OnError;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            StopGameStateUpdates();
            if (!string.IsNullOrEmpty(authToken))
                StartCoroutine(LeaveRoomCoroutine());
        }
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }

    private void HandleGameStateUpdate(ServerResponse response)
    {
        if (response != null)
        {
            try
            {
                currentGameState = ConvertToGameState(response);
                if (currentGameState != null)
                {
                    Debug.Log($"Game state updated - Status: {currentGameState.status}, Players: {currentGameState.players?.Length ?? 0}");
                    foreach (var player in currentGameState.players ?? new PlayerStatus[0])
                    {
                        Debug.Log($"Player {player.username}: Health={player.health}/{player.maxhealth}, State={player.state}, Attack={player.attackType}");
                    }
                }
                OnGameStateUpdated?.Invoke(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in HandleGameStateUpdate: {ex.Message}");
            }
        }
    }

    private GameState ConvertToGameState(ServerResponse response)
    {
        try
        {
            GameState gameState = new()
            {
                sessionId = response._id,
                status = response.gameStatus,
                currentTurnPlayerId = response.currentTurnCharacterId
            };

            if (response.users != null)
            {
                gameState.players = new PlayerStatus[response.users.Length];
                for (int i = 0; i < response.users.Length; i++)
                {
                    var user = response.users[i];
                    gameState.players[i] = new PlayerStatus
                    {
                        id = user._id,
                        username = user.characterName,
                        avatar = user.avatar,
                        maxhealth = user.maxHealth,
                        health = user.characterState?.health ?? user.maxHealth,
                        state = user.characterState?.state ?? "idle",
                        attackType = user.characterState?.attackAction ?? "",
                        attackDamage = user.characterState?.attackDamage ?? 0,
                        heal = user.characterState?.heal ?? 0,
                        bleedingCount = user.characterState?.bleedingCount ?? 0,
                        bleedingDamage = user.characterState?.bleedingDamage ?? 0,
                        stun = user.characterState?.stun ?? 0
                    };
                }
            }
            else
            {
                gameState.players = new PlayerStatus[0];
            }

            return gameState;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting to GameState: {ex.Message}");
            return null;
        }
    }

    public void JoinRoom(int code, string token)
    {
        if (isJoiningRoom) return;
        if (joinRoomCoroutine != null) StopCoroutine(joinRoomCoroutine);
        isJoiningRoom = true;
        if (string.IsNullOrEmpty(token)) return;
        authToken = token;
        joinRoomCoroutine = StartCoroutine(JoinRoomCoroutine(code));
    }

    public void StartSpectating()
    {
        StartCoroutine(GameStateUpdateCoroutine(2f));
    }

    public void ClearAuth()
    {
        authToken = null;
        StopGameStateUpdates();
    }

    public void LeaveRoom()
    {
        if (string.IsNullOrEmpty(authToken)) return;
        StartCoroutine(LeaveRoomCoroutine());
    }

    public void SetSessionId(string sessionId)
    {
        this.sessionId = sessionId;
        /*if (gameStateCoroutine != null)
            StopGameStateUpdates();*/
    }

    private void StopGameStateUpdates()
    {
        /*if (gameStateCoroutine != null)
        {
            StopCoroutine(gameStateCoroutine);
            gameStateCoroutine = null;
        }*/
    }

    private IEnumerator JoinRoomCoroutine(int code)
    {
        JoinRoomRequest requestData = new() { code = code };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        string url = $"{baseUrl}/room/{code}/add-spectator";
        
        using UnityWebRequest www = new(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + authToken);
        www.SetRequestHeader("Content-Type", "application/json");
        www.timeout = 15;

        yield return www.SendWebRequest();

        isJoiningRoom = false;

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Join Room failed - Result: {www.result}, Error: {www.error}");
            Debug.LogError($"Response Code: {www.responseCode}");
            Debug.LogError($"URL attempted: {url}");
            OnError?.Invoke($"Join Room failed: {www.error}");
        }
        else
        {
            try
            {
                string rawResponse = www.downloadHandler.text;
                Debug.Log($"Joined room: {url} - raw response: {rawResponse}");
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(rawResponse);
                OnJoinRoomResponse?.Invoke(response);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing Join Room response: " + ex.Message);
                Debug.LogError("Raw response: " + www.downloadHandler.text);
                OnError?.Invoke("Error parsing Join Room response: " + ex.Message);
            }
        }
    }

    private IEnumerator GameStateUpdateCoroutine(float interval)
    {
        while (!string.IsNullOrEmpty(authToken))
        {
            yield return StartCoroutine(GetGameStateCoroutine());
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator GetGameStateCoroutine()
    {
        string url = $"{baseUrl}/{sessionId}";

        using UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/{sessionId}");
        www.SetRequestHeader("Authorization", "Bearer " + authToken);
        www.timeout = 10;

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string rawResponse = www.downloadHandler.text;
                Debug.Log($"Getting state of {url} - raw response: {rawResponse}");
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(rawResponse);
                HandleGameStateUpdate(response);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing game state: " + ex.Message);
                OnError?.Invoke("Error parsing game state: " + ex.Message);
            }
        }
        else if (www.responseCode == 401)
        {
            Debug.LogError("Unauthorized access. Please check your auth token.");
            OnError?.Invoke("Unauthorized access. Please check your auth token.");
            authToken = null; // Clear the token on unauthorized access
            StopGameStateUpdates();
        }
        else
        {
            Debug.LogError("Game state fetch failed: " + www.error);
            OnError?.Invoke("Game state fetch failed: " + www.error);
        }
    }

    private IEnumerator LeaveRoomCoroutine()
    {
        using UnityWebRequest www = new($"{baseUrl}/{sessionId}/remove-spectator", "DELETE");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + authToken);
        www.timeout = 10;

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            Debug.Log("Successfully left the room.");
        else
            Debug.LogError("Failed to leave room: " + www.error);

        authToken = null;
        StopGameStateUpdates();
    }
}

[Serializable]
public class JoinRoomRequest
{
    public int code;
}

[Serializable]
public class ServerResponse
{
    public string _id;
    public string gameStatus;
    public string currentTurnCharacterId;
    public string roomCode;
    public ServerUser[] users;
    public string[] spectators;
    public string timeStamp;
    public int __v;
}

[Serializable]
public class ServerUser
{
    public string userid;
    public CharacterState characterState;
    public string characterName;
    public string @class;
    public string avatar;
    public string _id;
    public int maxHealth;
}

[Serializable]
public class CharacterState
{
    public string _id;
    public int health;
    public string state;
    public string attackAction;
    public int attackDamage;
    public int heal;
    public int bleedingCount;
    public int bleedingDamage;
    public int stun;
    public int __v;
}

