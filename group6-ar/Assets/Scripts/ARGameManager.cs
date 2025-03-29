using System.Collections.Generic;
using UnityEngine;

public class ARGameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject healthBarPrefab;
    public GameObject fireballPrefab;
    public GameObject lightningPrefab;
    public GameObject damageTextPrefab;
    public List<GameObject> playerModels;
    public List<GameObject> attackModels;

    private Dictionary<string, GameObject> players = new();

    void OnEnable()
    {
        WebSocketClient.OnMessageReceived += ProcessMessage;
    }

    void OnDisable()
    {
        WebSocketClient.OnMessageReceived -= ProcessMessage;
    }

    void ProcessMessage(string msg)
    {
        string[] parts = msg.Split(','); // SPAWN, PlayerName, PlayerModel, PosX, PosY, PosZ
        if (parts[0] == "SPAWN")
        {
            string playerName = parts[1];
            string playerModel = parts[2];
            Vector3 pos = new(float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]));
            SpawnPlayer(playerName, playerModel, pos);
        } else if (parts[0] == "ATTACK") // ATTACK, AttackType, AttackerPlayer, TargetPlayer, Damage
        {
            string attacker = parts[1];
            string target = parts[2];
            string attackType = parts[3];
            int damage = int.Parse(parts[4]);
            PlayAttackEffect(attackType, attacker, target, damage);
        } else if (parts[0] == "UPDATE_HP") // UPDATE_HP, PlayerName, NewHealth
        {
            string playerName = parts[1];
            int newHealth = int.Parse(parts[2]);
            UpdatePlayerStatus(playerName, newHealth);
        }
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
