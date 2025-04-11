using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private WebSocketManager wsManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARSession session;
    [SerializeField] private Camera Camera;
    [SerializeField] private GameObject gameArenaPrefab;
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private GameObject placeArenaBtn;
    [SerializeField] private TMP_InputField roomCodeInp;

    //private readonly Dictionary<string, PlayerController> players = new();
    //private readonly Vector3[] playerPositions = new();
    //private string currTurn = "";
    //private int turnCount = 0;

    private readonly List<ARRaycastHit> hits = new();
    private GameObject gameArena = null;
    private bool isPlacementValid = false;
    private bool isSetupComplete = false;
    private bool isGameActive = true; // CHANGE THIS TO FALSE 

    //private bool hasConnectedToServer = false;

    void Start()
    {
        if (raycastManager == null)
            raycastManager = FindObjectOfType<ARRaycastManager>();

        if (planeManager == null)
            planeManager = FindObjectOfType<ARPlaneManager>();

        if (session == null)
            session = FindObjectOfType<ARSession>();

        if (wsManager == null)
            wsManager = FindObjectOfType<WebSocketManager>();

        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();

        if (playerSpawner == null)
            playerSpawner = FindObjectOfType<PlayerSpawner>();

        if (Camera == null)
            Camera = FindObjectOfType<Camera>();

        if (networkManager != null)
            networkManager.OnConnectionStatusChanged += OnConnectionStatusChanged;

        //StartCoroutine(InitializeConnection());   UNCOMMENT THIS TO TEST CONNECTION

        ShowJoinRoomPanel(true);
        ShowPlaceArenaButton(false);
        //ShowGameInterface(false);
    }

    // TO BE CHECKED
    // here we might not need to update raycasts after finding a plane for the arena
    void Update()
    {
        if (isSetupComplete) return;

        isPlacementValid = false;

        Vector2[] screenPoints =
        {
            new(Screen.width / 2, Screen.height / 2),           // Center of the screen
            new(Screen.width / 4, Screen.height / 4),           // Top-left corner
            new(Screen.width * 3 / 4, Screen.height / 4),       // Top-right corner
            new(Screen.width * 3 / 4, Screen.height * 3 / 4),   // Bottom-right corner
            new(Screen.width / 4, Screen.height * 3 / 4),       // Bottom-left corner
        };

        foreach (var point in screenPoints)
        {
            if (raycastManager.Raycast(point, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                isPlacementValid = true;
                break;
            }
        }
    }

    void OnDestroy()
    {
        if (networkManager != null)
            networkManager.OnConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    public void OnPlaceArenaButtonClicked()
    {
        if (isPlacementValid)
        {
            PlaceArena();
            ShowPlaceArenaButton(false);
        }
        else
        {
            // SET STATUS MSG
        }

    }

    public void OnJoinRoomButtonClicked()
    {
        if (JoinRoom())
        {
            ShowJoinRoomPanel(false);
            ShowPlaceArenaButton(true);
        }
        else
        {
            // SET STATUS MSG 
        }
    }

    private bool JoinRoom()
    {
        if (roomCodeInp == null || string.IsNullOrEmpty(roomCodeInp.text))
        {
            Debug.LogError("Room code is empty");
            return false;
        }

        // UNCOMMENT THESE TO CONNECT TO THE SERVER
        /*string roomCode = roomCodeInp.text.Trim();
        wsManager.JoinRoom(roomCode);

        if (networkManager != null)
            networkManager.SetRoomCode(roomCode);*/

        //ShowGameInterface(true);

        return true;
    }

    private void PlaceArena()
    {
        if (!isPlacementValid || isSetupComplete) return;

        if (gameArena != null)
        {
            Destroy(gameArena);
            gameArena = null;
        }

        if (hits.Count > 0 && gameArenaPrefab != null)
        {
            Pose hitPose = hits[0].pose;
            Quaternion rotation = Quaternion.LookRotation(Camera.transform.forward, Vector3.up);
            gameArena = Instantiate(gameArenaPrefab, hitPose.position, rotation);
            gameArena.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            gameArena.tag = "GameArena";
            isSetupComplete = true;

            planeManager.enabled = false;
            //session.enabled = false;

            FocusCamOnArena(gameArena.transform);
            OnArenaPlaced();
        }
    }

    private void FocusCamOnArena(Transform transform)
    {
        Vector3 arenaCenter = transform.position;
        Vector3 cameraPos = arenaCenter;
        Camera.transform.position = cameraPos;
        Camera.transform.LookAt(arenaCenter);
    }

    private void OnArenaPlaced()
    {
        if (gameArena == null)
            gameArena = GameObject.FindGameObjectWithTag("GameArena");

        if (playerSpawner != null)
            playerSpawner.SpawnPlayers();
    }

    private void OnConnectionStatusChanged(bool isConnected)
    {
        isConnected = true; // REMOVE THIS LINE TO TEST CONNECTION
        if (!isConnected && isGameActive)
        {
            Debug.Log("Disconnected from server");
            ShowJoinRoomPanel(true);
            ShowPlaceArenaButton(false);
            //ShowGameInterface(false);
        }
    }

    private void ShowPlaceArenaButton(bool show)
    {
        if (placeArenaBtn != null)
            placeArenaBtn.SetActive(show);
    }

    private void ShowJoinRoomPanel(bool show)
    {
        if (joinRoomPanel != null)
            joinRoomPanel.SetActive(show);
    }

    /*private void ShowGameInterface(bool show)
    {
        if (gameInterface != null)
            gameInterface.SetActive(show);
    }

    /*public void SendTestGameState()
    {
        if (!hasConnectedToServer || wsManager == null)
            return;

        string testGameState = @"{
            ""type"": ""game_state"",
            ""players"": [
                {
                    ""id"": ""player1"",
                    ""name"": ""Player 1"",
                    ""avatar"": ""warrior"",
                    ""health"": 100,
                    ""pos"": {""x"": -0.5, ""y"": 0, ""z"": 0}
                },
                {
                    ""id"": ""player2"",
                    ""name"": ""Player 2"",
                    ""avatar"": ""mage"",
                    ""health"": 100,
                    ""pos"": {""x"": 0.5, ""y"": 0, ""z"": 0}
                }
            ],
            ""currTurn"": ""player1"",
            ""turnCount"": 1
        }";

        WebSocketManager.OnMsgReceived_Native(testGameState);
    }

    public void SendTestPlayerAction()
    {
        if (!hasConnectedToServer || wsManager == null)
            return;

        string testPlayerAction = @"{
            ""type"": ""player_action"",
            ""playerId"": ""player1"",
            ""targetId"": ""player2"",
            ""attackType"": ""attack"",
            ""attackName"": ""Attack1"",
            ""damage"": 20
        }";

        WebSocketManager.OnMsgReceived_Native(testPlayerAction);
    }

    /*private IEnumerator InitializeConnection()
    {
        Debug.Log("Connecting to WebSocket server...");

        if (wsManager != null)
            wsManager.ConnectToServer();

        yield return new WaitForSeconds(2.0f);

        if (wsManager != null && wsManager.IsWebSocketConnected())
        {
            Debug.Log("Connected to WebSocket server!");
            hasConnectedToServer = true;
            StartCoroutine(ShowPlaceArenaButtonWhenReady());
        }
        else
        {
            Debug.LogWarning("Failed to connect to WebSocket server. Retrying in 5 seconds...");
            yield return new WaitForSeconds(5.0f);
            StartCoroutine(InitializeConnection());
        }
    }

    private IEnumerator ShowPlaceArenaButtonWhenReady()
    {
        yield return new WaitForSeconds(3.0f);
        if (placeArenaButton != null)
            placeArenaButton.SetActive(true);
    }*/
}
