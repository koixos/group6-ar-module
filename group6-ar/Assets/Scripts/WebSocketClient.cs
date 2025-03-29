using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketClient : MonoBehaviour
{
    public static event Action<string> OnMessageReceived;
    private readonly ClientWebSocket webSocket = new();

    async void Start()
    {
        await webSocket.ConnectAsync(new System.Uri("ws://localhost:9001"), CancellationToken.None);
        Debug.Log("Connected to server");

        await ReceiveMsg();
    }

    public async Task SendMsg(string msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    async Task ReceiveMsg()
    {
        byte[] buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Debug.Log("Message from server: " + msg);

            // send the msg to the AR Manager 
            OnMessageReceived?.Invoke(msg);
        }
    }
}
