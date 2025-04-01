using System;
using System.Collections.Generic;
using UnityEngine;

public class ARGameManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    private GameObject player1Instance;
    private GameObject player2Instance;
    private Dictionary<int, CharacterController> characterControllers = new();

    private GameUpdateData currentGameState;

    private void Start()
    {
        if (WebSocketClient.Instance != null)
            WebSocketClient.Instance.OnMessageReceived += HandleGameUpdate;
    }

    void OnDisable()
    {
        //WebSocketClient.OnMessageReceived -= ProcessMessage;
    }

    private void HandleGameUpdate(string msg)
    {
        try
        {
            GameUpdateData updateData = JsonUtility.FromJson<GameUpdateData>(msg);
            UpdateGameStatus(updateData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while processing game state: {ex.Message}");
        }
    }

    private void UpdateGameStatus(GameUpdateData updateData)
    {
       
    }

    void SpawnPlayer(string playerName, string playerModel, Vector3 pos)
    {
        GameObject modelPrefab = playerModels.Find(x => x.name == playerModel);
        if (modelPrefab == null)
        {
            Debug.LogError("Model not found: " + playerModel);
            return;
        }

        GameObject player = Instantiate(modelPrefab, pos, Quaternion.identity);
        player.name = playerName;

        GameObject healthBar = Instantiate(healthBarPrefab, pos + Vector3.up * 2, Quaternion.identity);
        healthBar.transform.SetParent(player.transform);

        players[playerName] = player;
    }

    void PlayAttackEffect(string attackType, string attacker, string target, int damage)
    {
        if (!players.ContainsKey(attacker) || !players.ContainsKey(target))
        {
            Debug.LogError("Player not found");
            return;
        }

        GameObject targetPlayer = players[target];

        GameObject effect = null;
        if (attackType == "Fireball")
        {
            effect = Instantiate(fireballPrefab, targetPlayer.transform.position, Quaternion.identity);
        }
        else if (attackType == "Lightning")
        {
            effect = Instantiate(lightningPrefab, targetPlayer.transform.position, Quaternion.identity);
        }

        if (effect)
        {
            Destroy(effect, 2f);
        }

        ShowDamageEffect(targetPlayer, damage);
    }

    void UpdatePlayerStatus(string playerName, int newHealth)
    {
        if (!players.ContainsKey(playerName))
        {
            Debug.LogError("Player not found");
            return;
        }
        GameObject player = players[playerName];
        
        HealthBar healthBar = player.GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.UpdateHealth(newHealth);
        }
    }

    void ShowDamageEffect(GameObject target, int damageAmount)
    {
        GameObject damageText = Instantiate(damageTextPrefab, target.transform.position + Vector3.up * 2, Quaternion.identity);
        damageText.GetComponent<TextMesh>().text = " - " + damageAmount.ToString() + " HP";
        Destroy(damageText, 2f);
    }
}
