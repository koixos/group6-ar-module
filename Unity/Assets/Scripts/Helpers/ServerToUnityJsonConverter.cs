using Newtonsoft.Json.Linq;
using System.Xml;
using UnityEngine;

public class ServerToUnityJsonConverter : MonoBehaviour
{
    public static string Convert(string serverJson)
    {
        var serverObj = JObject.Parse(serverJson);
        var unityObj = new JObject
        {
            ["gameStatus"] = serverObj["gameStatus"],
            ["currentTurnCharacterId"] = serverObj["currentTurnCharacterId"]
        };
        
        var players = new JArray();
        foreach (var usr in serverObj["users"] as JArray)
        {
            var player = new JObject
            {
                ["id"] = usr["_id"],
                ["characterName"] = usr["characterName"],
                ["avatar"] = usr["avatar"],
                ["maxHealth"] = usr["maxHealth"],
                ["health"] = usr["characterState"]["health"],
                ["state"] = usr["characterState"]["state"],
                ["attackType"] = usr["characterState"]["attackAction"],
                ["attackDamage"] = usr["characterState"]["attackDamage"]
            };

            players.Add(player);
        }
        unityObj["players"] = players;

        return unityObj.ToString((Newtonsoft.Json.Formatting)Formatting.Indented);
    }
}
