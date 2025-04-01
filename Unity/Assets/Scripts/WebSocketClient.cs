using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using System.Collections;

public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance { get; private set; }
    public event Action<string> OnMessageReceived;

    [SerializeField] private string _url = "ws://localhost:9001";
    [SerializeField] private int reconnectAttempts = 3;

    private WebSocket webSocket;
    private bool isConnecting = false;
    private int currentReconnectAttempt = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // ConnectToServer();
    }

    private void Update()
    {
        if (webSocket != null)
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            webSocket.DispatchMessageQueue();
            #endif
        }
    }

    private async void OnApplicationQuit()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
            await webSocket.Close();
    }

    public async void ConnectToServer(string roomCode = "")
    {
        if (isConnecting) return;

        string url = _url;
        if (!string.IsNullOrEmpty(roomCode))
            url += "/join?room=" + roomCode;

        Debug.Log($"Connecting to the server: {url}");
        isConnecting = true;

        if (webSocket != null)
            await webSocket.Close();

        webSocket = new WebSocket(url);

        webSocket.OnOpen += () =>
        {
            Debug.Log("Connected established!");
            isConnecting = false;
            currentReconnectAttempt = 0;
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log($"WebSocket error: {e}");
            isConnecting = false;
            TryReconnect();
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log($"Connection closed! Code: {e}");
            isConnecting = false;
            TryReconnect();
        };

        webSocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            Debug.Log($"Received a message: {msg}");
            OnMessageReceived?.Invoke(msg);
        };

        try
        {
            await webSocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection error: {ex}");
            isConnecting = false;
            TryReconnect();
        }
    }

    public async void SendMsg(string msg)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
            await webSocket.SendText(msg);
        else
            Debug.LogWarning("Message could not be sent: WebSocket is not connected!");
    }

    private void TryReconnect()
    {
        if (currentReconnectAttempt < reconnectAttempts)
        {
            currentReconnectAttempt++;
            Debug.Log($"Reconnecting attempt {currentReconnectAttempt} /{reconnectAttempts}");
            StartCoroutine(ReconnectWithDelay(3f));
        }
        else
        {
            Debug.LogError("Exceeded the maximum reconnect attempt.");
        }
    }

    private IEnumerator ReconnectWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ConnectToServer();
    }
}
