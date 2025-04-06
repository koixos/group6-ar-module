using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomJoinUI : MonoBehaviour
{
    [SerializeField] private GameObject roomJoinPanel;
    [SerializeField] private GameObject scanningUI;
    [SerializeField] private GameObject connectionStatusPanel;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button scanQRBtn;
    [SerializeField] private TMP_InputField roomCodeInp;
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    private WebSocketManager wsManager;
    private GameController gameController;
    private QRScanner qRScanner;
    private Coroutine connectionCheckCoroutine;

    void Start()
    {
        wsManager = FindObjectOfType<WebSocketManager>();
        gameController = FindObjectOfType<GameController>();
        qRScanner = FindObjectOfType<QRScanner>();

        if (qRScanner == null && scanQRBtn != null)
        {
            qRScanner = gameObject.AddComponent<QRScanner>();
            qRScanner.Initialize();
        }

        if (joinBtn != null)
            joinBtn.onClick.AddListener(OnJoinButtonClicked);

        if (scanQRBtn != null)
            scanQRBtn.onClick.AddListener(OnScanQRButtonClicked);

        if (scanningUI != null)
            scanningUI.SetActive(false);

        if (connectionCheckCoroutine != null)
            StopCoroutine(connectionCheckCoroutine);

        connectionCheckCoroutine = StartCoroutine(CheckConnectionStatus());
    }

    void OnDestroy()
    {
        if (connectionCheckCoroutine != null)
            StopCoroutine(connectionCheckCoroutine);

        if (joinBtn != null)
            joinBtn.onClick.RemoveListener(OnJoinButtonClicked);

        if (scanQRBtn != null)
            scanQRBtn.onClick.RemoveListener(OnScanQRButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        if (roomCodeInp == null || string.IsNullOrEmpty(roomCodeInp.text))
        {
            ShowConnectionStatus("Please enter a room code", Color.red);
            return;
        }

        if (wsManager == null)
        {
            ShowConnectionStatus("WebSocketManager not found", Color.red);
            return;
        }

        if (!wsManager.IsWebSocketConnected())
        {
            ShowConnectionStatus("Not connected to server", Color.red);
            return;
        }

        string roomCode = roomCodeInp.text.Trim();
        wsManager.JoinRoom(roomCode);
        ShowConnectionStatus("Joining room " + roomCode + "...", Color.yellow);

        if (roomJoinPanel != null)
            roomJoinPanel.SetActive(false);
    }

    private void OnScanQRButtonClicked()
    {
        if (qRScanner == null)
        {
            ShowConnectionStatus("QR Scanner not found", Color.red);
            return;
        }

        if (scanningUI != null)
            scanningUI.SetActive(true);

        qRScanner.StartScanning(OnQRCodeDetected);
    }

    private void OnQRCodeDetected(string qrCode)
    {
        if (string.IsNullOrEmpty(qrCode))
        {
            ShowConnectionStatus("Invalid QR code", Color.red);
            return;
        }

        qRScanner.StopScanning();

        if (scanningUI != null)
            scanningUI.SetActive(false);

        if (roomCodeInp != null)
            roomCodeInp.text = qrCode;

        OnJoinButtonClicked();
    }

    public void ShowConnectionStatus(string message, Color color)
    {
        if (connectionStatusPanel == null || connectionStatusText == null)
            return;

        connectionStatusPanel.SetActive(true);
        connectionStatusText.text = message;
        connectionStatusText.color = color;
        
        if (color == Color.green)
            StartCoroutine(HideAfterDelay(connectionStatusPanel, 3f));
    }

    private IEnumerator CheckConnectionStatus()
    {
        while (true)
        {
            if (wsManager != null && !wsManager.IsWebSocketConnected())
                ShowConnectionStatus("Connected to server", Color.yellow);
            yield return new WaitForSeconds(2.0f);
        }
    }

    private IEnumerator HideAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            obj.SetActive(false);
    }
}

public class QRScanner : MonoBehaviour
{
    private bool isScanning = false;
    private System.Action<string> onQRCodeDetectedCallback;
    private UnityEngine.XR.ARFoundation.ARCameraManager cameraManager;

    public void Initialize()
    {
        cameraManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARCameraManager>();
    }

    public void StartScanning(System.Action<string> callback)
    {
        if (isScanning)
            return;

        onQRCodeDetectedCallback = callback;
        isScanning = true;

        StartCoroutine(ScanForQRCodes());
    }

    public void StopScanning()
    {
        isScanning = false;
        onQRCodeDetectedCallback = null;
    }

    private IEnumerator ScanForQRCodes()
    {
        yield return new WaitForSeconds(2.0f);

        if (isScanning && onQRCodeDetectedCallback != null)
        {
            string fakeRoomCode = "ROOM" + Random.Range(1000, 9999);
            onQRCodeDetectedCallback(fakeRoomCode);
        }
    }
}
