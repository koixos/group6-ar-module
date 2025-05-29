using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiCanvasPrefab;

    private Canvas playerCanvas;
    private TextMeshProUGUI nameText;
    private Slider healthBar;
    private Image healthBarFill;
    private Animator animator;
    private GameObject currentAttackEffect;

    private string id;
    private string username;
    private string avatar;
    private int health;
    private int maxHealth;
    private string currAnimTrig = "";

    void Awake()
    {
        animator = GetComponent<Animator>();
        //if (highlight == null)
        //highlight = transform.Find("Highlight")?.gameObject;
    }

    void LateUpdate()
    {
        if (playerCanvas != null && Camera.main != null)
        {
            Vector3 lookDir = Camera.main.transform.position - playerCanvas.transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                playerCanvas.transform.rotation = Quaternion.LookRotation(-lookDir);
        }
    }

    public void Initialize(string id, string username, string modelName, int health, int maxhealth)
    {
        this.id = id;
        this.username = username;
        this.avatar = modelName;
        this.health = health;
        this.maxHealth = maxhealth;

        CreatePlayerUI();
        UpdateUI();
    }

    public void Attack(string attackName)
    {
        PlayAnimation("attack");

        /*if (currentAttackEffect != null) Destroy(currentAttackEffect);
        GameObject attackPrefab = Resources.Load<GameObject>($"Attacks/{attackName}");
        if (attackPrefab == null) return;
        Vector3 attackPosition = transform.position + transform.forward * 2f;
        currentAttackEffect = Instantiate(attackPrefab, attackPosition, transform.rotation);
        //currentAttackEffect.transform.LookAt(transform.position - transform.forward * 10f);
        Destroy(currentAttackEffect, 2f);*/
    }

    public void Hurt(int damage)
    {
        SetHealth(health - damage);
        PlayAnimation("hurt");
        //ShowDamageAmount(damage);
    }

    public void UpdateHealth(int newHealth)
    {
        SetHealth(newHealth);
    }

    public void PlayIdleAnimation() => PlayAnimation("idle");
    public void PlayDefeatAnimation() => PlayAnimation("defeat");
    public void PlayVictoryAnimation() => PlayAnimation("victory");
    
    private void CreatePlayerUI()
    {
        if (uiCanvasPrefab != null)
        {
            GameObject canvasObj = Instantiate(uiCanvasPrefab, transform);
            playerCanvas = canvasObj.GetComponent<Canvas>();
            SetupUIReferences();
        }
        else
        {
            GameObject canvasObj = new("PlayerCanvas");
            canvasObj.transform.SetParent(transform);

            playerCanvas = canvasObj.AddComponent<Canvas>();
            playerCanvas.renderMode = RenderMode.WorldSpace;
            playerCanvas.worldCamera = Camera.main;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 0.01f;

            canvasObj.AddComponent<GraphicRaycaster>();

            CreateUIElements(canvasObj);
        }

        PositionUI();
    }

    private void CreateUIElements(GameObject canvasObj)
    {
        GameObject nameObj = new("Username");
        nameObj.transform.SetParent(canvasObj.transform);

        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = username;
        nameText.fontSize = 24;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;

        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(200, 50);
        nameRect.anchoredPosition = new Vector2(0, 60);

        GameObject healthBGObj = new("HealthBar");
        healthBGObj.transform.SetParent(canvasObj.transform);

        Image healthBG = healthBGObj.AddComponent<Image>();
        healthBG.color = new(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform healthBGRect = healthBG.GetComponent<RectTransform>();
        healthBGRect.sizeDelta = new Vector2(100, 12);
        healthBGRect.anchoredPosition = new Vector2(0, 30);

        GameObject healthFillObj = new("HealthBarFill");
        healthFillObj.transform.SetParent(healthBGObj.transform);

        healthBarFill = healthFillObj.AddComponent<Image>();
        healthBarFill.color = Color.green;

        RectTransform healthFillRect = healthBarFill.GetComponent<RectTransform>();
        healthFillRect.anchoredPosition = new Vector2(0, 0);
        healthFillRect.sizeDelta = new Vector2(0, 0);
        healthFillRect.anchorMin = new Vector2(0, 0);
        healthFillRect.anchorMax = new Vector2(1, 1);
        healthFillRect.offsetMin = new Vector2(0, 0);
        healthFillRect.offsetMax = new Vector2(0, 0);

        healthBar = healthBGObj.AddComponent<Slider>();
        healthBar.fillRect = healthFillRect;
        healthBar.minValue = 0;
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    private void SetupUIReferences()
    {
        if (playerCanvas != null)
        {
            nameText = playerCanvas.GetComponentInChildren<TextMeshProUGUI>();
            healthBar = playerCanvas.GetComponentInChildren<Slider>();

            if (healthBar != null && healthBar.fillRect != null)
                healthBarFill = healthBar.fillRect.GetComponent<Image>();
        }
    }

    private void PositionUI()
    {
        if (playerCanvas != null)
        {
            playerCanvas.transform.SetLocalPositionAndRotation(new Vector3(0, 2.5f, 0), Quaternion.identity);
            playerCanvas.transform.localScale = Vector3.one;
        }
    }

    private void UpdateUI()
    {
        if (nameText != null) nameText.text = username;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
            UpdateHealthBarColor();
        }
    }

    private void UpdateHealthBarColor()
    {
        if (healthBarFill == null) return;
        float percentage = (float)health / maxHealth;
        if (percentage > 0.6f)
            healthBarFill.color = Color.green;
        else if (percentage > 0.3f)
            healthBarFill.color = Color.yellow;
        else
            healthBarFill.color = Color.red;
    }

    private void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateUI();
    }

    private void PlayAnimation(string trigger)
    {
        if (animator == null) return;
        if (currAnimTrig == trigger) return;

        ResetAllTriggers();
        animator.SetTrigger(trigger);
        currAnimTrig = trigger;
    }

    private void ResetAllTriggers()
    {
        if (animator == null) return;

        string[] triggers = { "idle", "attack", "hurt", "victory", "defeat", "heal", "bleed", "stun" };
        foreach (string trigger in triggers)
        {
            animator.ResetTrigger(trigger);
        }

        currAnimTrig = "";
    }

    /*private void ShowDamageAmount(int damage)
    {
        // Implement damage number display logic here
        Debug.Log($"Damage: {damage}");
    }*/
}
