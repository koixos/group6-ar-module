using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class WebSocketManager : MonoBehaviour
{
    public delegate void MsgReceivedCallback(string msg);
    public event Action<string> OnMessageReceived;

    private const string PLUGIN_NAME = "UnityWebSocketPlugin";
    private const string INSTANCE_NAME = "WebSocketManager";
    private const string DEFAULT_SERVER_URL = "ws://localhost:8080";

    [SerializeField] private GameController gameController;
    private static WebSocketManager instance;

    [DllImport(PLUGIN_NAME)]
    private static extern void SetMsgCallback(MsgReceivedCallback callback);

    [DllImport(PLUGIN_NAME)]
    private static extern void InitializeWebSocket(string url);

    [DllImport(PLUGIN_NAME)]
    private static extern void SendMsg(string msg);

    [DllImport(PLUGIN_NAME)]
    private static extern void CloseWebSocket();

    [DllImport(PLUGIN_NAME)]
    private static extern bool IsConnected();

    public static WebSocketManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new(INSTANCE_NAME);
                instance = gameObject.AddComponent<WebSocketManager>();
                DontDestroyOnLoad(gameObject);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetMsgCallback(OnMsgReceived_Native);

        if (gameController == null)
            gameController = FindObjectOfType<GameController>();
    }

    void OnDestroy()
    {
        if (instance == this)
            CloseWebSocket();
    }

    [AOT.MonoPInvokeCallback(typeof(MsgReceivedCallback))]
    public static void OnMsgReceived_Native(string msg)
    {
        if (instance != null)
        {
            instance.OnMessageReceived?.Invoke(msg);
            NetworkManager networkManager = NetworkManager.Instance;
            if (networkManager != null)
                networkManager.QueueMsg(() => instance.ProcessMessage(msg));
            else
                instance.ProcessMessage(msg);
        }
    }

    public void Connect(string url)
    {
        InitializeWebSocket(url);
    }

    public void ConnectToServer()
    {
        Connect(DEFAULT_SERVER_URL);
    }

    public void JoinRoom(string code)
    {
        if (!IsConnected())
        {
            Debug.LogWarning("Cannot join room: WebSocket not connected");
            return;
        }

        JoinRoomMessage joinMsg = new()
        {
            type = "join_room",
            roomCode = code
        };

        string json = JsonConvert.SerializeObject(joinMsg);
        Send(json);
    }

    public void Send(string msg)
    {
        if (IsConnected())
            SendMessage(msg);
        else
            Debug.LogWarning("Cannot send message: WebSocket not connected");
    }

    public void Disconnect()
    {
        CloseWebSocket();
    }

    public bool IsWebSocketConnected()
    {
        return IsConnected();
    }
    
    private void ProcessMessage(string json)
    {
        try
        {
            BaseMessage baseMsg = JsonConvert.DeserializeObject<BaseMessage>(json);
            if (baseMsg == null || string.IsNullOrEmpty(baseMsg.type))
            {
                Debug.LogError("Invalid message format or missing type");
                return;
            }

            switch (baseMsg.type)
            {
                case "game_state":
                    GameStateMessage gameState = JsonConvert.DeserializeObject<GameStateMessage>(json);
                    if (gameController != null)
                        gameController.UpdateGameState(gameState);
                    else
                        Debug.LogError("GameController is not set");
                    break;

                case "player_action":
                    PlayerActionMessage playerAction = JsonConvert.DeserializeObject<PlayerActionMessage>(json);
                    if (gameController != null)
                        gameController.HandlePlayerAction(playerAction);
                    else
                        Debug.LogError("GameController is not set");
                    break;

                case "connection_status":
                    ConnectionStatusMessage connectionStatus = JsonConvert.DeserializeObject<ConnectionStatusMessage>(json);
                    Debug.Log($"Connection status: {connectionStatus.status}");

                    if(NetworkManager.Instance != null)
                        NetworkManager.Instance.InvokeConnectionStatusChanged(connectionStatus.status == "connected");
                    break;

                default:
                    Debug.LogWarning($"Unknown message type: {baseMsg.type}");
                    break;
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"Failed to process message: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}");
        }
    }

    [Serializable]
    public class BaseMessage
    {
        public string type;
    }

    [Serializable]
    public class JoinRoomMessage : BaseMessage
    {
        public string roomCode;
    }

    [Serializable]
    public class GameStateMessage : BaseMessage
    {
        public PlayerState[] players;
        public string currTurn;
        public int turnCount;
    }

    [Serializable]
    public class PlayerActionMessage : BaseMessage
    {
        public string playerId;
        public string targetId;
        public string attackType;
        public string attackName;
        public int damage;
    }

    [Serializable]
    public class ConnectionStatusMessage : BaseMessage
    {
        public string status;
    }

    [Serializable]
    public class PlayerState
    {
        public string id;
        public string name;
        public string avatar;
        public int health;
        public Vector3Serializable pos;
    }

    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
