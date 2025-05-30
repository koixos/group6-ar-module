using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public Slider healthBar;
    public Image healthBarFill;
    public GameObject iconRow;

    [Header("Status Icons")]
    public GameObject iconHeal;    // Heart icon
    public GameObject iconBleed;   // Droplet icon
    public GameObject iconStun;    // Hurricane icon
    public TextMeshProUGUI healInfoText;
    public TextMeshProUGUI bleedInfoText;
    public TextMeshProUGUI stunInfoText;

    void OnValidate()
    {
        if (nameText == null)
            Debug.LogError("nameText is not assigned in UserUI!");
        if (healthBar == null)
            Debug.LogError("healthBar is not assigned in UserUI!");
        if (healthBarFill == null)
            Debug.LogError("healthBarFill is not assigned in UserUI!");
        if (iconRow == null)
            Debug.LogError("iconRow is not assigned in UserUI!");
        if (iconHeal == null)
            Debug.LogError("iconHeal is not assigned in UserUI!");
        if (iconBleed == null)
            Debug.LogError("iconBleed is not assigned in UserUI!");
        if (iconStun == null)
            Debug.LogError("iconStun is not assigned in UserUI!");
        if (healInfoText == null)
            Debug.LogError("healInfoText is not assigned in UserUI!");
        if (bleedInfoText == null)
            Debug.LogError("bleedInfoText is not assigned in UserUI!");
        if (stunInfoText == null)
            Debug.LogError("stunInfoText is not assigned in UserUI!");
    }
} 