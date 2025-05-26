using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject arenaPrefab;
    //[SerializeField] private GameObject joinRoomPanel;
    //[SerializeField] private GameObject statusText;

    private readonly List<ARRaycastHit> hits = new();
    private GameObject gameArena = null;
    private bool arenaPlaced = false;
    private bool isGameActive = false;

    void Start()
    {
        if (gameController == null) gameController = FindObjectOfType<GameController>();
        if (raycastManager == null) raycastManager = FindObjectOfType<ARRaycastManager>();
        if (cam == null) cam = FindObjectOfType<Camera>();

        isGameActive = true;
        //ShowJoinRoomPanel(true);
        //OnJoinRoomButtonClicked();
    }

    void Update()
    {
        if (!arenaPlaced && isGameActive && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            PlaceArena(Input.GetTouch(0).position);
    }

    /*public void OnWebSocketMsg(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        if (gameController == null) return;
        gameController.ProcessGameState(json);
    }*/

    private void PlaceArena(Vector2 screenPos)
    {
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

    /*public void OnJoinRoomButtonClicked()
    {
        isGameActive = true;
        ShowJoinRoomPanel(false);
    }

    public void RestartGame()
    {
        ResetArena();
        ShowJoinRoomPanel(true);
        isGameActive = false;  
    }
    
    private void OnConnectionStatusChanged(bool isConnected)
    {
        if (!isConnected && isGameActive)
        {
            ResetArena();
            ShowJoinRoomPanel(true);
            isGameActive = false;
        }
    }

    private void ShowJoinRoomPanel(bool show)
    {
        if (joinRoomPanel != null)
            joinRoomPanel.SetActive(show);
    }

    private void ResetArena()
    {
        arenaPlaced = false;
        if (gameArena != null)
        {
            Destroy(gameArena);
            gameArena = null;
        }

        if (gameController != null)
            gameController.gameObject.SetActive(false);
    }    

    private void ShowStatus(string message)
    {
        if (statusText != null && statusText.TryGetComponent<TextMeshProUGUI>(out var text))
        {
            text.text = message;
            CancelInvoke(nameof(ClearStatus));
            Invoke(nameof(ClearStatus), 3f);
        }
    }

    private void ClearStatus()
    {
        if (statusText != null && statusText.TryGetComponent<TextMeshProUGUI>(out var text))
            text.text = string.Empty;
    }

    /*void OnDestroy()
    {
        if (networkManager != null)
            networkManager.OnConnectionStatusChanged -= OnConnectionStatusChanged;
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
            networkManager.SetRoomCode(roomCode)

        //ShowGameInterface(true);

        return true;
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
    }*/
}
