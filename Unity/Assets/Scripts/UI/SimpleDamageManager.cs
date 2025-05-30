using UnityEngine;

public class SimpleDamageManager : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;

    private static SimpleDamageManager instance;
    public static SimpleDamageManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SimpleDamageManager>();
                if (instance == null)
                {
                    GameObject go = new("DamageManager");
                    instance = go.AddComponent<SimpleDamageManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamage(int damage, Vector3 position, DamageType type = DamageType.Normal)
    {
        Debug.Log($"ShowDamage called: damage={damage}, position={position}");

        if (damageTextPrefab == null)
        {
            Debug.LogError("Damage text prefab is not assigned!");
            return;
        }

        Debug.Log($"Prefab found: {damageTextPrefab.name}");

        Vector3 randomOffset = new(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

        Vector3 spawnPos = position + randomOffset;
        Debug.Log($"Spawning damage text at: {spawnPos}");

        GameObject damageObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Damage object created: {damageObj.name}");

        if (damageObj.TryGetComponent<SimpleDamageText>(out var damageScript))
        {
            Color damageColor = GetDamageColor(type);
            Debug.Log($"Calling ShowDamage on script with color: {damageColor}");
            damageScript.ShowDamage(damage, damageColor);
        }
        else
        {
            Debug.LogError("SimpleDamageText component not found on instantiated object!");
        }
    }

    private Color GetDamageColor(DamageType type)
    {
        return type switch
        {
            DamageType.Normal => Color.red,
            DamageType.Bleed => Color.yellow,
            DamageType.Heal => Color.green,
            _ => Color.red
        };
    }
}

public enum DamageType
{
    Normal,
    Bleed,
    Heal,
}
