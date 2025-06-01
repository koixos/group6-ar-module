using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MainSceneManager : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private APIManager apiManager;

    [Header("UI")]
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private TextMeshProUGUI errorTxt;
    [SerializeField] private TMP_InputField roomCodeInp;
    [SerializeField] private Button connectBtn;

    [Header("Game Components")]
    [SerializeField] private GameController gameController;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject arenaPrefab;

    private readonly List<ARRaycastHit> hits = new();
    private GameObject gameArena = null;
    private bool arenaPlaced = false;
    private bool isGameActive = false; 
    private string token;

    void Start()
    {
        GetTokenFromIntent();
        StartCoroutine(InitializeEverything());
        ShowJoinRoomPanel(false);
        isGameActive = true;
    }

    void Update()
    {
        if (!arenaPlaced && isGameActive && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            PlaceArena(Input.GetTouch(0).position);

        if (isGameActive && arenaPlaced)
            StartGameStateTest();
    }

    void OnDestroy()
    {
        CleanupEventListeners();
    }

    public void OnJoinRoomButtonClicked()
    {
        if (roomCodeInp == null || string.IsNullOrEmpty(roomCodeInp.text.Trim()))
        {
            ShowError("Room code cannot be empty.");
            return;
        }

        if (!int.TryParse(roomCodeInp.text.Trim(), out int roomCode))
        {
            ShowError("Please enter a valid number.");
            return;
        }

        Debug.Log($"Attempting to join room: {roomCode}");

        ShowError("");
        SetConnectBtnInteractable(false);
        StartCoroutine(JoinRoomWithDelay(roomCode));
    }

    public void ResetArena()
    {
        if (gameArena != null)
        {
            Destroy(gameArena);
            gameArena = null;
            arenaPlaced = false;
        }
    }

    public void StartGameStateTest()
    {
        StartCoroutine(TestGameStatesFromFile());
        isGameActive = false;
    }

    public void GetTokenFromIntent()
    {
        try
        {
            using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            string token = intent.Call<string>("getStringExtra", "user_token");
            if (!string.IsNullOrEmpty(token))
            {
                Debug.Log($"Token received from Intent: {token}");
                this.token = token;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get token from Intent: {e.Message}");
        }

    }

    public void ReceiveToken(string token)
    {
        this.token = token;
        Debug.Log("Received token: " + token);
    }

    private void InitializeUI()
    {
        if (connectBtn != null)
            connectBtn.onClick.AddListener(OnJoinRoomButtonClicked);

        if (apiManager != null)
        {
            apiManager.OnJoinRoomResponse += HandleJoinRoomResponse;
            apiManager.OnGameStateUpdated += HandleGameStateUpdate;
            apiManager.OnError += HandleApiError;
        }

        if (errorTxt != null)
            errorTxt.gameObject.SetActive(false);
    }

    private void FindComponents()
    {
        if (apiManager == null) apiManager = FindObjectOfType<APIManager>();
        if (gameController == null) gameController = FindObjectOfType<GameController>();
        if (raycastManager == null) raycastManager = FindObjectOfType<ARRaycastManager>();
        if (cam == null) cam = FindObjectOfType<Camera>();
    }

    private bool IsApiManagerReady()
    {
        return apiManager != null && apiManager.isActiveAndEnabled;
    }

    private void CleanupEventListeners()
    {
        if (connectBtn != null)
            connectBtn.onClick.RemoveListener(OnJoinRoomButtonClicked);

        if (apiManager != null)
        {
            apiManager.OnJoinRoomResponse -= HandleJoinRoomResponse;
            apiManager.OnGameStateUpdated -= HandleGameStateUpdate;
            apiManager.OnError -= HandleApiError;
        }
    }

    private void HandleJoinRoomResponse(ServerResponse response)
    {
        try
        {
            if (response != null)
            {
                var gameState = ConvertToGameState(response);
                if (gameState != null)
                {
                    isGameActive = true;
                    ShowJoinRoomPanel(false);

                    Debug.Log($"Joined room successfully with session id: {gameState.sessionId}");
                    Debug.Log($"Game status: {gameState.status}, Players: {gameState.players?.Length ?? 0}");

                    apiManager.SetSessionId(gameState.sessionId);
                    apiManager.StartSpectating();
                }
                else
                {
                    ShowError("Failed to convert server response.");
                    SetConnectBtnInteractable(true);
                }
            }
            else
            {
                ShowError("Received null response from server.");
                SetConnectBtnInteractable(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing join room response: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            ShowError("An error occurred while processing the response.");
            SetConnectBtnInteractable(true);
        }
    }

    private void HandleApiError(string err)
    {
        Debug.LogError($"API Error: {err}");
        ShowError($"Connection failed: {err}");
        SetConnectBtnInteractable(true);
    }

    private void HandleGameStateUpdate(ServerResponse response)
    {
        try
        {
            if (response != null)
            {
                var gameState = ConvertToGameState(response);
                if (gameState != null)
                    gameController.ProcessGameState(gameState, false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling game state update: {ex.Message}");
            ShowError("Error handling game state update.");
        }
    }

    private void ShowJoinRoomPanel(bool show)
    {
        if (joinRoomPanel != null)
            joinRoomPanel.SetActive(show);
    }

    private void ShowError(string message)
    {
        if (errorTxt != null)
        {
            if (string.IsNullOrEmpty(message))
            {
                errorTxt.gameObject.SetActive(false);
            }
            else
            {
                errorTxt.text = message;
                errorTxt.gameObject.SetActive(true);
                Debug.LogWarning($"UI Error: {message}");
            }
        }
    }

    private void SetConnectBtnInteractable(bool interactable)
    {
        if (connectBtn != null)
            connectBtn.interactable = interactable;
    }

    private void PlaceArena(Vector2 screenPos)
    {
        if (arenaPrefab == null) return;

        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            var pose = hits[0].pose;
            gameArena = Instantiate(arenaPrefab, pose.position, pose.rotation);
        }
        else
        {
            Vector3 pos = cam.transform.position + cam.transform.forward * 2f;
            pos.y -= 1.5f;
            Quaternion rot = Quaternion.Euler(45f, cam.transform.eulerAngles.y, 0);
            gameArena = Instantiate(arenaPrefab, pos, rot);
        }

        if (gameController != null)
        {
            arenaPlaced = gameController.SetArena(gameArena);
            if (APIManager.Instance != null)
            {
                var currentState = APIManager.Instance.GetCurrentGameState();
                if (currentState != null)
                {
                    Debug.Log("Processing initial game state after arena placement");
                    gameController.ProcessGameState(currentState, true);
                }
                else
                {
                    Debug.LogWarning("No current game state available after arena placement");
                }
            }
        }
    }

    private string[] SplitJsonObjects(string jsonContent)
    {
        List<string> jsonObjects = new();
        int braceCount = 0;
        int startIndex = 0;
        bool inString = false;
        bool escapeNext = false;

        for (int i = 0; i < jsonContent.Length; i++)
        {
            char c = jsonContent[i];

            if (escapeNext)
            {
                escapeNext = false;
                continue;
            }

            if (c == '\\')
            {
                escapeNext = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (!inString)
            {
                if (c == '{')
                {
                    if (braceCount == 0)
                        startIndex = i;
                    braceCount++;
                }
                else if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        string jsonObject = jsonContent.Substring(startIndex, i - startIndex + 1);
                        jsonObjects.Add(jsonObject.Trim());
                    }
                }
            }
        }

        return jsonObjects.ToArray();
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

    private IEnumerator TestGameStatesFromFile()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("game_state_data");
        string json;
        try
        {
            json = jsonAsset.text;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading file: {e.Message}");
            yield break;
        }

        string[] gameStates = SplitJsonObjects(json);
        for (int i = 0; i < gameStates.Length; i++)
        {
            string currGS = gameStates[i].Trim();
            if (string.IsNullOrEmpty(currGS)) continue;

            try
            {
                Debug.Log(i + 1);
                GameState resp = JsonUtility.FromJson<GameState>(currGS);
                Debug.Log(resp.players[0].attackDamage);
                Debug.Log(resp.players[1].attackDamage);
                if (i == 0)
                    gameController.ProcessGameState(resp, true);
                else
                    gameController.ProcessGameState(resp, false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing game state {i + 1}: {ex.Message} - {currGS}");
                ShowError($"Error processing game state {i + 1}: {ex.Message}");
            }

            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Finished processing game states from file.");
    }

    private IEnumerator InitializeEverything()
    {
        FindComponents();

        while (apiManager == null || !IsApiManagerReady())
            yield return new WaitForSeconds(0.1f);

        Debug.Log("All components ready, initializing UI");

        InitializeUI();
        ShowJoinRoomPanel(true);
    }

    private IEnumerator JoinRoomWithDelay(int roomCode)
    {
        yield return new WaitForSeconds(0.2f);

        try
        {
            apiManager.JoinRoom(roomCode, token);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error joining room: {ex.Message}");
            ShowError("An error occurred while trying to join the room.");
            SetConnectBtnInteractable(true);
        }
    }
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
    public int heal = 0;
    public int bleedingCount = 0;
    public int bleedingDamage = 0;
    public int stun = 0;
}
