using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string baseUrl = "http://192.168.137.1:3001/api/ar";
    private string sessionId;
    private string authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJfaWQiOiI2ODFhMDY0NjcxNmEzOGI0MDkyZmUzMDciLCJpYXQiOjE3NDg0MzM5NTUsImV4cCI6MTc0ODQzNzU1NX0.2NlDNGV9WNB6u9u_XmTNseQedhnq2KzVXgUPdWPpurU";

    private Coroutine joinRoomCoroutine;
    private bool isJoiningRoom = false;

    public event Action<ServerResponse> OnJoinRoomResponse;
    public event Action<ServerResponse> OnGameStateUpdated;
    public event Action<string> OnError;

    void OnDestroy()
    {
        StopGameStateUpdates();
        if (!string.IsNullOrEmpty(authToken))
            StartCoroutine(LeaveRoomCoroutine());
    }

    public void JoinRoom(int code)
    {
        if (isJoiningRoom) return;
        if (joinRoomCoroutine != null) StopCoroutine(joinRoomCoroutine);
        isJoiningRoom = true;
        joinRoomCoroutine = StartCoroutine(JoinRoomCoroutine(code));
    }

    public void StartSpectating()
    {
        StartCoroutine(GameStateUpdateCoroutine(2f));
    }

    public void GetCurrentGameState()
    {
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Auth token is not set. Cannot fetch game state.");
            OnError?.Invoke("Auth token is not set. Cannot fetch game state.");
            return;
        }

        StartCoroutine(GetGameStateCoroutine());
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
                OnGameStateUpdated?.Invoke(response);
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

