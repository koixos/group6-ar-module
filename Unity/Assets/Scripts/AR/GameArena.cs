using UnityEngine;

public class GameArena : MonoBehaviour
{
    [SerializeField] private Transform player1Pos;
    [SerializeField] private Transform player2Pos;
    [SerializeField] private float arenaScale = 1.0f;
    [SerializeField] private GameObject arenaFloor;
    [SerializeField] private GameObject arenaBoundary;

    void Start()
    {
        if (player1Pos == null)
        {
            GameObject pos1 = new("Player1Position");
            pos1.transform.SetParent(transform);
            pos1.transform.localPosition = new Vector3(-0.5f, 0, 0);
            player1Pos = pos1.transform;
        }

        if (player2Pos == null)
        {
            GameObject pos2 = new("Player2Position");
            pos2.transform.SetParent(transform);
            pos2.transform.localPosition = new Vector3(0.5f, 0, 0);
            player2Pos = pos2.transform;
        }
    }

    public void SetArenaScale(float scale)
    {
        arenaScale = scale;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public Transform GetPlayerPosition(int playerIndex)
    {
        return playerIndex == 0 ? player1Pos : player2Pos;
    }
}
