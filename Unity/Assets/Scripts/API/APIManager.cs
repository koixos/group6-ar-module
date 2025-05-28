using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string baseUrl = "http://192.168.137.1:3001/api/ar";
    private string sessionId = "";
    private string authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJfaWQiOiI2ODFhMDY0NjcxNmEzOGI0MDkyZmUzMDciLCJpYXQiOjE3NDg0MTQ4NjAsImV4cCI6MTc0ODQxODQ2MH0.NO98js39DZXVjRiUeJJ4v2sNjkej-Qt3GuomRM29xXw";

    private Coroutine gameStateCoroutine;

    public event Action<JoinRoomResponse> OnJoinRoomResponse;
    public event Action<GameState> OnGameStateUpdated;
    public event Action<string> OnError;

    void OnDestroy()
    {
        StopGameStateUpdates();
        if (!string.IsNullOrEmpty(authToken))
            StartCoroutine(LeaveRoomCoroutine());
    }

    public void JoinRoom(int code)
    {
        StartCoroutine(JoinRoomCoroutine(code));
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(authToken);
    }

    public string GetAuthToken()
    {
        return authToken;
    }

    public void ClearAuth()
    {
        authToken = null;
        StopGameStateUpdates();
    }
    
    public void GetGameState()
    {
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Auth token is not set. Cannot fetch game state.");
            OnError?.Invoke("Auth token is not set. Cannot fetch game state.");
            return;
        }

        StartCoroutine(GetGameStateCoroutine());
    }

    public void StartGameStateUpdates(float interval = 1f)
    {
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Auth token is not set. Cannot start game state updates.");
            return;
        }

        if (gameStateCoroutine != null)
        {
            StopCoroutine(gameStateCoroutine);
        }

        gameStateCoroutine = StartCoroutine(GameStateUpdateCoroutine(interval));
    }

    public void LeaveRoom()
    {
        if (string.IsNullOrEmpty(authToken)) return;
        StartCoroutine(LeaveRoomCoroutine());
    }

    public void StopGameStateUpdates()
    {
        if (gameStateCoroutine != null)
        {
            StopCoroutine(gameStateCoroutine);
            gameStateCoroutine = null;
        }
    }

    private IEnumerator JoinRoomCoroutine(int code)
    {
        JoinRoomRequest requestData = new() { code = code };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        string url = $"{baseUrl}/room/{code}/add-spectator";
        Debug.Log($"Joined room: {url}");

        using UnityWebRequest www = new(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + authToken);
        www.SetRequestHeader("Content-Type", "application/json");
        www.timeout = 15;

        yield return www.SendWebRequest();
        
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
                JoinRoomResponse response = JsonUtility.FromJson<JoinRoomResponse>(www.downloadHandler.text);
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
        using UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/{sessionId}");
        www.SetRequestHeader("Authorization", "Bearer " + authToken);
        www.timeout = 10;

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                GameState gameState = JsonUtility.FromJson<GameState>(www.downloadHandler.text);
                OnGameStateUpdated?.Invoke(gameState);
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
public class JoinRoomResponse
{
    public string gameState;
}

[Serializable]
public class GameState
{
    public string sessionId;
    public string status;
    public string currentTurnPlayerId;
    public PlayerStatus[] players;
}

[Serializable]
public class PlayerStatus
{
    public string id;
    public string username;
    public string avatar;
    public int health;
    public int maxhealth;
    public string state;
    public string attackType = "";
    public int attackDamage = 0;
}

