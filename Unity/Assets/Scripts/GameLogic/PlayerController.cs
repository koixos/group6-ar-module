using System.Linq;
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
    private Camera cam;

    private string id;
    private string username;
    private string avatar;
    private int health;
    private int maxHealth;
    private string currAnimTrig = "";

    void Awake()
    {
        cam = FindARCam();
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (playerCanvas != null && cam != null)
        {
            Vector3 lookDir = cam.transform.position - playerCanvas.transform.position;
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
    
    private Camera FindARCam()
    {
        if (Camera.main != null) return Camera.main;
        var arCam = FindObjectOfType<UnityEngine.XR.ARFoundation.ARCameraManager>();
        if (arCam != null) return arCam.GetComponent<Camera>();
        Debug.LogWarning("No AR camera found, using default camera.");
        return null;
    }

    private void CreatePlayerUI()
    {
        if (uiCanvasPrefab != null)
        {
            Debug.Log($"[{username}] Using prefab UI");

            GameObject canvasObj = Instantiate(uiCanvasPrefab, transform);
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            canvasObj.transform.localScale = Vector3.one;

            playerCanvas = canvasObj.GetComponent<Canvas>();
            if (playerCanvas != null)
            {
                playerCanvas.renderMode = RenderMode.WorldSpace;
                playerCanvas.worldCamera = cam;

                if (playerCanvas.TryGetComponent<CanvasScaler>(out var scaler))
                    scaler.enabled = false;
            
                if (playerCanvas.TryGetComponent<RectTransform>(out var rectTransform))
                    rectTransform.sizeDelta = new Vector2(3f, 2f);
            }

            SetupUIReferences();
        }
        else
        {
            GameObject canvasObj = new("PlayerCanvas");
            canvasObj.transform.SetParent(transform);

            playerCanvas = canvasObj.AddComponent<Canvas>();
            playerCanvas.renderMode = RenderMode.WorldSpace;
            playerCanvas.worldCamera = cam;

            RectTransform canvasRect = playerCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(3f, 2f);

            CreateUIElements(canvasObj);
        }

        PositionUI();

        Debug.Log($"[{username}] UI Validation:");
        Debug.Log($"  - Canvas: {(playerCanvas != null ? "OK" : "MISSING")}");
        Debug.Log($"  - NameText: {(nameText != null ? nameText.text : "MISSING")}");
        Debug.Log($"  - HealthBar: {(healthBar != null ? $"Value: {healthBar.value}/{healthBar.maxValue}" : "MISSING")}");

        if (playerCanvas != null)
        {
            Debug.Log($"  - Canvas Size: {playerCanvas.GetComponent<RectTransform>().sizeDelta}");
            Debug.Log($"  - Canvas Scale: {playerCanvas.transform.localScale}");
            Debug.Log($"  - Canvas Position: {playerCanvas.transform.localPosition}");
        }
    }

    private void CreateUIElements(GameObject canvasObj)
    {
        GameObject nameObj = new("UserName");
        nameObj.transform.SetParent(canvasObj.transform);

        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = username;
        nameText.fontSize = 0.5f;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;

        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(2.5f, 0.6f);
        nameRect.anchoredPosition = new Vector2(0, 0.8f);

        Debug.Log($"[{username}] Username text created: '{nameText.text}', fontSize: {nameText.fontSize}");

        GameObject healthBGObj = new("HealthBar");
        healthBGObj.transform.SetParent(canvasObj.transform);

        Image healthBG = healthBGObj.AddComponent<Image>();
        healthBG.color = new(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform healthBGRect = healthBG.GetComponent<RectTransform>();
        healthBGRect.sizeDelta = new Vector2(2f, 0.3f);
        healthBGRect.anchoredPosition = new Vector2(0, 0.2f);

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
        healthBar.interactable = false;

        Debug.Log($"[{username}] Health bar created: {health}/{maxHealth}");
    }

    private void SetupUIReferences()
    {
        if (playerCanvas == null) return;

        for (int i = 0; i < playerCanvas.transform.childCount; i++)
        {
            Transform child = playerCanvas.transform.GetChild(i);
            Debug.Log($"[{username}] Child {i}: {child.name} - Components: {string.Join(", ", child.GetComponents<Component>().Select(c => c.GetType().Name))}");
        }

        nameText = playerCanvas.GetComponentInChildren<TextMeshProUGUI>();
        healthBar = playerCanvas.GetComponentInChildren<Slider>();
        
        if (healthBar.fillRect != null)
            healthBarFill = healthBar.fillRect.GetComponent<Image>();

        Transform usernameObj = playerCanvas.transform.Find("UserName");
        if (usernameObj != null)
        {
            if (usernameObj.TryGetComponent<TextMeshProUGUI>(out var manualText))
            {
                nameText = manualText;
                Debug.Log($"[{username}] NameText set manually: '{nameText.text}'");
            }
        }
        else
        {
            Debug.LogWarning($"[{username}] No 'Username' object found");
        }

        Transform healthObj = playerCanvas.transform.Find("HealthBar");
        if (healthObj != null)
        {
            Debug.Log($"[{username}] Found HealthBar object by name");
            if (healthObj.TryGetComponent<Slider>(out var manualSlider))
            {
                healthBar = manualSlider;
                Debug.Log($"[{username}] HealthBar set manually");
            }
        }
        else
        {
            Debug.LogWarning($"[{username}] No 'HealthBar' object found");
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
        if (nameText != null) 
            nameText.text = username;
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
