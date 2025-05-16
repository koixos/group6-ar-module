using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebSocketBridge : MonoBehaviour
{
    [SerializeField] private GameObject obj;

    private const string PLUGIN_NAME = "UnityWebSocketPlugin";
    private static readonly ConcurrentQueue<string> messageQueue = new();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MessageCallback(IntPtr msgPtr);

    [DllImport(PLUGIN_NAME)]
    private static extern void InitializeWebSocket(string url, MessageCallback cb);

    //[DllImport(PLUGIN_NAME)]
    //private static extern void SendMsg(string msg);

    [DllImport(PLUGIN_NAME)]
    private static extern void CloseWebSocket();

    [DllImport(PLUGIN_NAME)]
    private static extern bool IsConnected();

    [AOT.MonoPInvokeCallback(typeof(MessageCallback))]
    static void OnMessageReceived(IntPtr msgPtr)
    {
        try
        {
            if (msgPtr == IntPtr.Zero) return;
            string msg = Marshal.PtrToStringAnsi(msgPtr);
            messageQueue.Enqueue(msg);
        }
        catch (Exception e)
        {
            Debug.LogError("Callback exception: " + e);
        }
    }

    void Start()
    {
        obj = GameObject.Find("GameController");
        if (obj == null)
        {
            Debug.LogError("GameController not found.");
            return;
        }
        InitializeWebSocket("ws://localhost:8080", OnMessageReceived);
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out var msg))
        {
            Debug.Log("[Main Thread] Processing message: " + msg);
            if (obj.TryGetComponent<GameController>(out var controller))
                controller.OnWebSocketMsg(msg);
            else
                Debug.LogError("GameController not found or missing component.");
        }
    }

    void OnDestroy()
    {
        CloseWebSocket();
    }
}
