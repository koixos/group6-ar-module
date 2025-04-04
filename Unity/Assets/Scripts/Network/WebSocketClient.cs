using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WebSocketClient : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID
    private const string PLUGIN_NAME = "__Internal";
#else
    private const string PLUGIN_NAME = "WebSocketPlugin";
#endif

    [DllImport(PLUGIN_NAME)]
    private static extern void InitializeWebSocket(string url, string roomCode);

    [DllImport(PLUGIN_NAME)]
    private static extern void SendMessageToWebSocket(string message);

    [DllImport(PLUGIN_NAME)]
    private static extern void CloseWebSocket();

    [DllImport(PLUGIN_NAME)]
    private static extern bool IsConnected();

    [SerializeField] private string _url = "ws://localhost:8080";

    private bool isConnected = false;
    public bool IsClientConnected => isConnected;

    private GameController gameController;
    private Queue<string> messageQueue = new();

    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
        lock (messageQueue)
        {
            while (messageQueue.Count > 0)
            {
                ProcessMessage(messageQueue.Dequeue());
            }
        }

        isConnected = IsConnected();
    }

    void OnDestroy()
    {
        CloseWebSocket();
    }

    public void ConnectToServer()
    {
        Debug.Log($"Connecting to the server: {_url}");
        InitializeWebSocket(_url, gameController.roomCode);
    }

    public void JoinRoom(string code)
    {
        if (!isConnected)
        {
            Debug.LogError("WebSocket is not connected. Cannot join room.");
            return;
        }

        JoinRoomMessage joinMsg = new JoinRoomMessage
        {
            type = "join_room",
            roomCode = code
        };

        string json = JsonConvert.SerializeObject(joinMsg);
        SendMessage(json);
        Debug.Log($"Joining room with code: {code}");
    }

    public void OnMessageReceived(string message)
    {
        lock (messageQueue)
        {
            messageQueue.Enqueue(message);
        }
    }

    private void ProcessMessage(string json)
    {
        try
        {
            BaseMessage baseMsg = JsonConvert.DeserializeObject<BaseMessage>(json);

            switch (baseMsg.type)
            {
                case "game_state":
                    GameStateMessage gameState = JsonConvert.DeserializeObject<GameStateMessage>(json);
                    gameController.UpdateGameState(gameState);
                    break;

                case "player_action":
                    PlayerActionMessage playerAction = JsonConvert.DeserializeObject<PlayerActionMessage>(json);
                    gameController.HandlePlayerAction(playerAction);
                    break;

                case "connection_status":
                    ConnectionStatusMessage connectionStatus = JsonConvert.DeserializeObject<ConnectionStatusMessage>(json);
                    Debug.Log($"Connection status: {connectionStatus.status}");
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
    private class BaseMessage
    {
        public string type;
    }

    [Serializable]
    private class JoinRoomMessage : BaseMessage
    {
        public string roomCode;
    }

    [Serializable]
    private class GameStateMessage : BaseMessage
    {
        public PlayerState[] players;
        public string currTurn;
        public int turnCount;
    }

    [Serializable]
    private class PlayerActionMessage : BaseMessage
    {
        public string playerId;
        public string targetId;
        public string action;
        public int damage;
    }

    [Serializable]
    private class ConnectionStatusMessage : BaseMessage
    {
        public string status;
    }

    [Serializable]
    public class PlayerState
    {
        public string id;
        public string name;
        public string avatar;
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
