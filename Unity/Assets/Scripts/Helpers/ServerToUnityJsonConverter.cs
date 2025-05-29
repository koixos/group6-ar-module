using Newtonsoft.Json.Linq;
using System;
using System.Xml;
using UnityEngine;

public class ServerToUnityJsonConverter : MonoBehaviour
{
    public static string Convert(string serverJson)
    {
        try
        {
            var serverObj = JObject.Parse(serverJson);

            var unityObj = new JObject
            {
                ["sessionId"] = serverObj["_id"],
                ["status"] = serverObj["gameStatus"],
                ["currentTurnPlayerId"] = serverObj["currentTurnCharacterId"]
            };

            var players = new JArray();

            if (serverObj["users"] is JArray usersArray)
            {
                foreach (var user in usersArray)
                {
                    var characterState = user["characterState"];

                    var player = new JObject
                    {
                        ["id"] = user["_id"],
                        ["username"] = user["characterName"],
                        ["avatar"] = user["avatar"],
                        ["health"] = characterState["health"],
                        ["maxhealth"] = user["maxHealth"],
                        ["state"] = characterState["state"],
                        ["attackType"] = characterState["attackAction"],
                        ["attackDamage"] = characterState["attackDamage"],
                        ["bleedingCount"] = characterState["bleedingCount"],
                        ["bleedingDamage"] = characterState["bleedingDamage"],
                        ["heal"] = characterState["heal"]
                        ["stun"] = characterState["stun"]
                    };
                    players.Add(player);
                }
            }

            unityObj["players"] = players;

            Debug.Log($"Converted JSON: {unityObj}");
            return unityObj.ToString();
        }
        catch (Exception ex)
        {
            Debug.LogError($"JSON Conversion Error: {ex.Message}");
            Debug.LogError($"Input JSON: {serverJson}");
            return null;
        }
    }
}
