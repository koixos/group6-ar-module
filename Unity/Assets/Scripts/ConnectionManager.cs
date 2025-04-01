using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManaegr : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomCodeInp;
    [SerializeField] private Button connectBtn;
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        connectBtn.onClick.AddListener(ConnectToRoom);

        if (WebSocketClient.Instance != null)
            WebSocketClient.Instance.OnMessageReceived += OnMessageReceived;
        else
            Debug.LogError("Could not find WebSocketClient!");
    }

    private void OnDestroy()
    {
        if (WebSocketClient.Instance != null)
            WebSocketClient.Instance.OnMessageReceived -= OnMessageReceived;
    }

    private void ConnectToRoom()
    {
        string roomCode = roomCodeInp.text;
        if (string.IsNullOrEmpty(roomCode))
        {
            UpdateStatus("Please provide a room code!");
            return;
        }

        UpdateStatus("Connecting...");

        WebSocketClient.Instance.ConnectToServer(roomCode);
    }

    private void OnMessageReceived(string msg)
    {
        if (msg.Contains("connected") || msg.Contains("welcome"))
        {
            UpdateStatus("Connected!");
            StartCoroutine(HideConnectionPanelAfterDelay(1.5f));
        }
    }

    private IEnumerator HideConnectionPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        connectionPanel.SetActive(false);
    }

    private void UpdateStatus(string stat)
    {
        if (statusText != null)
            statusText.text = stat;
    }
}
