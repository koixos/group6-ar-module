using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private GameArena gameArena;

    private GameObject player1;
    private GameObject player2;

    public void SpawnPlayers()
    {
        if (gameArena == null)
        {
            gameArena = FindObjectOfType<GameArena>();
            if (gameArena == null)
            {
                Debug.LogError("GameArena not found in the scene.");
                return;
            }
        }

        SpawnPlayer1();
        SpawnPlayer2();
    }

    private void SpawnPlayer1()
    {
        if (player1Prefab == null)
        {
            Debug.LogError("Player 1 prefab is not assigned!");
            return;
        }
        
        Transform spawnPos = gameArena.GetPlayerPosition(0);
        player1 = Instantiate(player1Prefab, spawnPos.position, spawnPos.rotation);
        player1.transform.SetParent(spawnPos);
    }

    private void SpawnPlayer2()
    {
        if (player2Prefab == null)
        {
            Debug.LogError("Player 2 prefab is not assigned!");
            return;
        }

        Transform spawnPos = gameArena.GetPlayerPosition(1);
        player2 = Instantiate(player2Prefab, spawnPos.position, spawnPos.rotation);
        player2.transform.SetParent(spawnPos);
    }

}
