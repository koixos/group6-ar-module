using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private WebSocketManager wsManager;
    [SerializeField] private float pingInterval = 5f;
    [SerializeField] private float reconnectDelay = 3f;
    [SerializeField] private int maxQeueuedMsgs = 60;
    [SerializeField] private float processingDelay = 0.016f; // ~60 FPS

    private bool shouldReconnect = true;
    private string currRoomCode = "";
    private float avrgLatency = 0f;
    private int msgCount = 0;
    private int droppedMsgs = 0;
    private Queue<Action> msgQueue = new();

    public event Action<bool> OnConnectionStatusChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (wsManager == null)
            wsManager = FindObjectOfType<WebSocketManager>();
    }

    void Start()
    {
        StartCoroutine(MonitorConnection());
        StartCoroutine(ProcessMessageQueue());
    }

    public void QueueMsg(Action msgAction)
    {
        lock (msgQueue)
        {
            while (msgQueue.Count >= maxQeueuedMsgs)
            {
                msgQueue.Dequeue();
                droppedMsgs++;
            }

            msgQueue.Enqueue(msgAction);
        }
    }

    public void SetRoomCode(string roomCode)
    {
        currRoomCode = roomCode;
    }

    private IEnumerator ProcessMessageQueue()
    {
        while (true)
        {
            int processedThisFrame = 0;

            lock (msgQueue)
            {
                while (msgQueue.Count > 0 && processedThisFrame < 3)
                {
                    Action msgAction = msgQueue.Dequeue();
                    try
                    {
                        msgAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error processing network message: {e.Message}");
                    }
                    processedThisFrame++;
                }
            }

            yield return new WaitForSeconds(processingDelay);
        }
    }

    public void RecordMsgLatency(float latency)
    {
        if (msgCount == 0)
        {
            avrgLatency = latency;
        }
        else
        {
            avrgLatency = (avrgLatency * msgCount + latency) / (msgCount + 1);
        }
        msgCount++;

        if (msgCount % 10 == 0)
            AdjustNetworkSettings();
    }

    public void AdjustNetworkSettings()
    {
        if (avrgLatency > 200)
            processingDelay = 0.033f; // ~30 FPS
        else if (avrgLatency > 100)
            processingDelay = 0.025f; // ~40 FPS
        else
            processingDelay = 0.016f; // ~60 FPS
    }

    public string GetNetworkStats()
    {
        return $"Latency: {avrgLatency:F1}ms | Dropped: {droppedMsgs} | Queue: {msgQueue.Count}";
    }

    public void InvokeConnectionStatusChanged(bool isConnected)
    {
        OnConnectionStatusChanged?.Invoke(isConnected);
    }

    private IEnumerator MonitorConnection()
    {
        while (true)
        {
            bool isConnected = wsManager != null && wsManager.IsWebSocketConnected();

            if (!isConnected && shouldReconnect)
            {
                Debug.Log("Attempting to reconnect...");
                wsManager.ConnectToServer();

                if (!string.IsNullOrEmpty(currRoomCode))
                {
                    yield return new WaitForSeconds(1.0f);
                    wsManager.JoinRoom(currRoomCode);
                }

                OnConnectionStatusChanged?.Invoke(false);
                yield return new WaitForSeconds(reconnectDelay);
            }
            else if (isConnected)
            {
                OnConnectionStatusChanged?.Invoke(true);
                yield return new WaitForSeconds(pingInterval);
                wsManager.Send("{\"type\":\"ping\"}");
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
