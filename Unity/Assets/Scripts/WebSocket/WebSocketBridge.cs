using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebSocketBridge : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private string sessionId = "681c8ee256a702d8c1500b40";
    [SerializeField] private string authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJfaWQiOiI2ODFhMDY0NjcxNmEzOGI0MDkyZmUzMDciLCJpYXQiOjE3NDgxODI0NzYsImV4cCI6MTc0ODE4NjA3Nn0.uvalja8l1fQAsfViYiys7KI1VGVSkPorSNYReIiGWss";
    [SerializeField] private float pollingInterval = 2f;

    private const string ENDPOINT_URL = "http://10.1.230.32:3001/api/ar/";

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        StartCoroutine(PollGameState());
    }

    IEnumerator PollGameState()
    {
        while (true)
        {
            yield return FetchGameState();
            yield return new WaitForSeconds(pollingInterval);
        }
    }

    IEnumerator FetchGameState()
    {
        string url = ENDPOINT_URL + sessionId;

        using UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + authToken);

        Debug.Log("Sending request to: " + url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Game state fetch failed: " + www.error);
            Debug.LogError("Response code: " + www.responseCode);
        }
        else
        {
            string json = www.downloadHandler.text;
            Debug.Log("Received Game State: " + json);
            gameController.ProcessGameState(json);
        }
    }
}
