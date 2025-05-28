using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GetReqExp : MonoBehaviour
{
    [Header("Test Config")]
    public string testUrl = "http://192.168.216.66:3001/api/ar/";
    [SerializeField] private string sessionId = "6834d831a2a5277d4778051e";

    [Header("UI Refs")]
    public Button testBtn;
    public Text statTxt;

    void Start()
    {
        if (testBtn != null)
            testBtn.onClick.AddListener(TestConn);

        if (statTxt != null)
            statTxt.text = "Ready";

        Invoke(nameof(TestConn), 2f);
    }

    public void TestConn()
    {
        StartCoroutine(TestGetReq());
    }

    IEnumerator TestGetReq()
    {
        if (statTxt != null)
            statTxt.text = "Connecting...";

        Debug.Log($"Testing GET req to: {testUrl} + {sessionId}");

        using UnityWebRequest www = UnityWebRequest.Get(testUrl + sessionId);
        www.timeout = 10;

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Response: {www.downloadHandler.text}");
            if (statTxt != null)
                statTxt.text = $"Code: {www.responseCode}\nResponse: {www.downloadHandler.text}";
            var converted = ServerToUnityJsonConverter.Convert(www.downloadHandler.text);
            Debug.Log($"Converted Response: {converted}");
        }
        else
        {
            Debug.LogError($"Error: {www.error}");
            Debug.LogError($"Response Code: {www.responseCode}");
            if (statTxt != null)
                statTxt.text = $"Error: {www.error}\nCode: {www.responseCode}";
        }
    }

    [ContextMenu("Test Get Request")]
    public void TestFromInspector()
    {
        TestConn();
    }

    [Space(10)]
    [Header("Quick Test")]
    public bool testOnStart = true;

}
