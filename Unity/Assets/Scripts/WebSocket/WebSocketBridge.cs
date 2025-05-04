using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebSocketBridge : MonoBehaviour
{
    [DllImport("UnityWebSocketPlugin")]
    private static extern void InitializeWebSocket(string url);

    [DllImport("UnityWebSocketPlugin")]
    private static extern void SendMsg(string msg);

    [DllImport("UnityWebSocketPlugin")]
    private static extern void CloseWebSocket();

    [DllImport("UnityWebSocketPlugin")]
    private static extern bool IsConnected();

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeWebSocket("ws://192.168.1.5:8080");
        Invoke("SendTestMsg", 3f);
#endif
    }

    void SendTestMsg()
    {
        if (IsConnected())
            SendMsg("{\"type\": \"hello\", \"content\": \"Hello world!\"}");
        else
            Debug.Log("WebSocket connection failed.");
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        CloseWebSocket();
#endif
    }
}
