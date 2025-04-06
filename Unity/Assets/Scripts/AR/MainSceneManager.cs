using System.Collections;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private ARSetup arSetup;
    [SerializeField] private WebSocketManager wsManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject placeArenaButton;

    private bool hasConnectedToServer = false;

    void Start()
    {
        if (arSetup == null)
            arSetup = FindObjectOfType<ARSetup>();

        if (wsManager == null)
            wsManager = FindObjectOfType<WebSocketManager>();

        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();

        if (gameController == null)
            gameController = FindObjectOfType<GameController>();

        if (placeArenaButton != null)
            placeArenaButton.SetActive(false);

        StartCoroutine(InitializeConnection());
    }

    private IEnumerator InitializeConnection()
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

    public void OnPlaceArenaButtonClicked()
    {
        if (arSetup != null)
            arSetup.PlaceArena();

        if (placeArenaButton != null)
            placeArenaButton.SetActive(false);
    }

    public void SendTestGameState()
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

    private IEnumerator ShowPlaceArenaButtonWhenReady()
    {
        yield return new WaitForSeconds(3.0f);
        if (placeArenaButton != null)
            placeArenaButton.SetActive(true);
    }
}
