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

    void Start()
    {
        StartCoroutine(InitializeEverything());
    }

    void Update()
    {
        if (!arenaPlaced && isGameActive && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            PlaceArena(Input.GetTouch(0).position);
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

    private IEnumerator InitializeEverything()
    {
        FindComponents();

        while (apiManager == null || !IsApiManagerReady())
            yield return new WaitForSeconds(0.1f);

        Debug.Log("All components ready, initializing UI");

        InitializeUI();
        ShowJoinRoomPanel(true);
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
   201 }

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

    private IEnumerator JoinRoomWithDelay(int roomCode)
    {
        yield return new WaitForSeconds(0.2f);

        try
        {
            apiManager.JoinRoom(roomCode);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error joining room: {ex.Message}");
            ShowError("An error occurred while trying to join the room.");
            SetConnectBtnInteractable(true);
        }
    }

    private void HandleJoinRoomResponse(JoinRoomResponse response)
    {
        try
        {
            if (response != null && !string.IsNullOrEmpty(response.gameState))
            {
                Debug.Log($"Joined room successfully");
                var gameState = ServerToUnityJsonConverter.Convert(response.gameState);

                if (gameState != null)
                {
                    isGameActive = true;
                    ShowJoinRoomPanel(false);

                    GameState state = JsonUtility.FromJson<GameState>(gameState);
                    if (state != null)
                        gameController.ProcessGameState(state);
                }
                else
                {
                    ShowError("Failed to parse game state from server response.");
                    SetConnectBtnInteractable(true);
                }
            }
            else
            {
                ShowError($"Failed to join room");
                SetConnectBtnInteractable(true);
            }
       0 }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing join room response: {ex.Message}");
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

    private void HandleGameStateUpdate(GameState gameState)
    {
        gameController.ProcessGameState(gameState);
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
        if (arenaPrefab == null)
        {
            Debug.LogError("Arena prefab not assigned!");
            return;
        }

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
            gameController.SetArena(gameArena);
            arenaPlaced = true;
        }
    }
}
