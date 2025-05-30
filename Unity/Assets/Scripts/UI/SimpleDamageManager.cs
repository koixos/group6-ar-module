using UnityEngine;

public enum DamageType
{
    Damage,
    Heal,
    Bleed,
    Stun
}

public class SimpleDamageManager : MonoBehaviour
{
    public static SimpleDamageManager Instance { get; private set; }

    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SimpleDamageManager is ready.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamage(int damage, Vector3 position, DamageType type = DamageType.Damage)
    {
        if (floatingDamagePrefab == null)
        {
            Debug.LogError("Floating damage prefab is not assigned!");
            return;
        }

        Debug.Log($"Creating damage text: {damage} of type {type} at position {position}");

        // Create the damage text object
        GameObject damageObj = Instantiate(floatingDamagePrefab, position, Quaternion.identity);

        if (damageObj.TryGetComponent<Canvas>(out var dmgCanvas))
        {
            dmgCanvas.renderMode = RenderMode.WorldSpace;
            dmgCanvas.worldCamera = Camera.main;
        }

        damageObj.transform.LookAt(Camera.main.transform);
        damageObj.transform.Rotate(0, 180F, 0); // Ensure it faces the camera correctly

        // Get the SimpleDamageText component
        if (damageObj.TryGetComponent<SimpleDamageText>(out var damageText))
        {
            // Set color based on damage type
            Color textColor = GetColorForDamageType(type);
            
            // Add a prefix based on type
            string prefix = type switch
            {
                DamageType.Damage => "-",
                DamageType.Heal => "+",
                DamageType.Bleed => "🩸",
                DamageType.Stun => "⚡",
                _ => ""
            };
            
            // Show the damage with appropriate color and prefix
            damageText.ShowDamage(damage, textColor);
            
            Debug.Log($"Damage text created successfully: {prefix}{damage}");
        }
        else
        {
            Debug.LogError("SimpleDamageText component not found on prefab!");
            Destroy(damageObj);
        }
    }

    private Color GetColorForDamageType(DamageType type)
    {
        return type switch
        {
            DamageType.Damage => Color.red,
            DamageType.Heal => Color.green,
            DamageType.Bleed => new Color(0.8f, 0f, 0f), // Dark red
            DamageType.Stun => Color.yellow,
            _ => Color.white
        };
    }
}
