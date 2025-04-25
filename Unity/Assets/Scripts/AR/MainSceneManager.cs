using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject gameArenaPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Camera Camera;
    [SerializeField] private GameController gameController;

    private readonly List<ARRaycastHit> hits = new();
    private GameObject gameArena = null;
    private bool arenaPlaced = false;

    /*[SerializeField] private WebSocketManager wsManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private ARSession session;
    
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private GameObject placeArenaBtn;
    [SerializeField] private TMP_InputField roomCodeInp;

    
    
    private bool isGameActive = true; // CHANGE THIS TO FALSE 
    */
    //private bool hasConnectedToServer = false;

    void Start()
    {
        if (raycastManager == null)
            raycastManager = FindObjectOfType<ARRaycastManager>();

        /*if (session == null)
            session = FindObjectOfType<ARSession>();

        if (wsManager == null)
            wsManager = FindObjectOfType<WebSocketManager>();

        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();

        if (playerSpawner == null)
            playerSpawner = FindObjectOfType<PlayerSpawner>();
        */
        if (Camera == null)
            Camera = FindObjectOfType<Camera>();

        /*if (networkManager != null)
            networkManager.OnConnectionStatusChanged += OnConnectionStatusChanged;
        */
        //StartCoroutine(InitializeConnection());   UNCOMMENT THIS TO TEST CONNECTION

        //ShowJoinRoomPanel(true);
        //ShowPlaceArenaButton(false);
        //ShowGameInterface(false);
    }

    // TO BE CHECKED
    // here we might not need to update raycasts after finding a plane for the arena
    void Update()
    {
        if (!arenaPlaced && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceArena(Input.GetTouch(0).position);
            arenaPlaced = true;
            //playerSpawner.SpawnPlayers(gameArena);
            //return;
        }
    }

    private void PlaceArena(Vector2 screenPos)
    {
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            var pose = hits[0].pose;
            gameArena = Instantiate(gameArenaPrefab, pose.position, pose.rotation);
        }
        else
        {
            Vector3 pos = Camera.transform.position + Camera.transform.forward * 2f;
            pos.y -= 1.5f;
            Quaternion rot = Quaternion.Euler(45f, Camera.transform.eulerAngles.y, 0);
            gameArena = Instantiate(gameArenaPrefab, pos, rot);
        }

        if (gameController != null)
        {
            gameController.gameObject.SetActive(true);
            gameController.SetArena(gameArena);
        }
    }

    /*void OnDestroy()
    {
        if (networkManager != null)
            networkManager.OnConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    /*public void OnPlaceArenaButtonClicked()
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

    }*/

    /*public void OnJoinRoomButtonClicked()
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
*/
    

    private void ResetArena()
    {
        arenaPlaced = false;
        if (gameArena != null)
        {
            Destroy(gameArena);
            gameArena = null;
        }
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
