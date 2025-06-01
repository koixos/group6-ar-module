using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiCanvasPrefab;
    [HideInInspector] public UserUI userUI;

    private Canvas playerCanvas;
    private Animator animator;
    private Camera cam;

    private string username;
    private int health;
    private int maxHealth;
    private string currAnimTrig = "";

    public int CurrentHealth => health;

    void Awake()
    {
        cam = FindARCam();
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (playerCanvas != null && playerCanvas.renderMode == RenderMode.WorldSpace)
        {
            Vector3 lookDir = cam.transform.position - playerCanvas.transform.position;
            if (lookDir != Vector3.zero)
                playerCanvas.transform.rotation = Quaternion.LookRotation(-lookDir);
        }
    }

    public void Initialize(string id, string username, string modelName, int health, int maxhealth)
    {
        this.username = username;
        this.health = health;
        this.maxHealth = maxhealth;

        CreatePlayerUI();
        SetupUI();
    }

    public void Attack(string attackName)
    {
        PlayAnimation("attack");
    }

    public void Hurt(int damage)
    {
        Debug.Log($"[{username}] Taking damage: {damage}");
        int oldHealth = health;
        UpdateHealth(health - damage);
        PlayAnimation("hurt");
        
        if (damage >= 0)
        {
            Vector3 damagePosition = transform.position + Vector3.up * 2.5f;
                        
            if (cam != null)
            {
                Vector3 cameraDirection = (cam.transform.position - transform.position).normalized;
                damagePosition += cameraDirection * 0.5f;
            }
            
            Debug.Log($"[{username}] Showing damage: {damage} at position: {damagePosition}");
            SimpleDamageManager.Instance?.ShowDamage(damage, damagePosition, DamageType.Damage);
        }
    }

    public void Heal(int amount)
    {
        UpdateHealth(health + amount);
        Vector3 healPosition = transform.position + Vector3.up * 2.5f;
        SimpleDamageManager.Instance.ShowDamage(amount, healPosition, DamageType.Heal);
    }

    public void UpdateHealth(int newHealth)
    {
        SetHealth(newHealth);
        if (userUI != null && userUI.healthBar != null)
        {
            userUI.healthBar.value = health;
            UpdateHealthBarColor();
        }
    }

    public void ShowBleed(int turns, int dmg)
    {
        if (userUI != null && userUI.iconBleed != null)
        {
            userUI.iconBleed.SetActive(true);
            if (userUI.iconBleed.TryGetComponent<IconSetup>(out var iconSetup))
                iconSetup.ShowInfo($"{turns}/-{dmg}");
        }
    }

    public void HideBleed()
    {
        if (userUI != null && userUI.iconBleed != null)
        {
            userUI.iconBleed.SetActive(false);
            if (userUI.iconBleed.TryGetComponent<IconSetup>(out var iconSetup))
                iconSetup.HideInfo();
        }
    }

    public void ShowHeal(int amount)
    {
        if (userUI != null && userUI.iconHeal != null)
        {
            userUI.iconHeal.SetActive(true);
            if (userUI.iconHeal.TryGetComponent<IconSetup>(out var iconSetup))
                iconSetup.ShowInfo($"+{amount}");
        }
    }

    public void HideHeal()
    {
        if (userUI != null && userUI.iconHeal != null)
        {
            userUI.iconHeal.SetActive(false);
            if (userUI.iconHeal.TryGetComponent<IconSetup>(out var iconSetup))
                iconSetup.HideInfo();
        }
    }

    public void ShowStun(int turns)
    {
        if (userUI != null && userUI.iconStun != null)
        {
            userUI.iconStun.SetActive(true);
            if (userUI.iconStun.TryGetComponent<IconSetup>(out var iconSetup))
                iconSetup.ShowInfo($"{turns}");
        }
    }

    public void HideStun()
    {
        if (userUI != null && userUI.iconStun != null)
        {
            userUI.iconStun.SetActive(false);
            if (userUI.iconStun.TryGetComponent<IconSetup>(out var iconSetup))
                iconSetup.HideInfo();
        }
    }

    public void PlayIdleAnimation() => PlayAnimation("idle");
    public void PlayDefeatAnimation() => PlayAnimation("defeat");
    public void PlayVictoryAnimation() => PlayAnimation("victory");
    
    private Camera FindARCam()
    {
        if (Camera.main != null) return Camera.main;
        var arCam = FindObjectOfType<UnityEngine.XR.ARFoundation.ARCameraManager>();
        if (arCam != null) return arCam.GetComponent<Camera>();
        Debug.LogWarning("No AR camera found");
        return null;
    }

    private void CreatePlayerUI()
    {
        Debug.Log($"[{username}] Starting CreatePlayerUI");
        if (uiCanvasPrefab != null)
        {
            GameObject canvasObj = Instantiate(uiCanvasPrefab, transform);
            Debug.Log($"[{username}] Canvas instantiated: {canvasObj.name}");
            
            canvasObj.transform.SetLocalPositionAndRotation(new(0f, 2.3f, 0f), Quaternion.identity);
            canvasObj.transform.localScale = Vector3.one * 0.01f;

            playerCanvas = canvasObj.GetComponent<Canvas>();
            if (playerCanvas != null)
            {
                playerCanvas.renderMode = RenderMode.WorldSpace;
                playerCanvas.worldCamera = cam;
                Debug.Log($"[{username}] Canvas setup complete");
            }

            if (canvasObj.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 1f;
                Debug.Log($"[{username}] CanvasScaler added");
            }

            if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log($"[{username}] GraphicRaycaster added");
            }

            userUI = canvasObj.GetComponent<UserUI>();
            if (userUI == null)
            {
                Debug.LogError($"[{username}] UserUI component not found on instantiated canvas!");
                return;
            }

            LogIconRowSetup();
        }
        else
        {
            Debug.LogError($"[{username}] No uiCanvasPrefab assigned!");
        }
    }

    private void CheckIconVisuals(GameObject icon, string iconName)
    {
        if (icon == null) return;

        var rectTransform = icon.GetComponent<RectTransform>();
        var image = icon.GetComponentInChildren<Image>();
        var text = icon.GetComponentInChildren<TextMeshProUGUI>();

        Debug.Log($"[{username}] {iconName} Icon properties:" +
            $"\nRectTransform - Size: {rectTransform.sizeDelta}, Scale: {rectTransform.localScale}" +
            $"\nImage - {(image != null ? $"Sprite: {(image.sprite != null ? image.sprite.name : "NULL")}, Color: {image.color}" : "No Image component")}" +
            $"\nText - {(text != null ? $"Text: {text.text}, Size: {text.fontSize}, Color: {text.color}" : "No Text component")}");
    }

    private void LogIconRowSetup()
    {
        if (userUI.iconRow != null)
        {
            var rectTransform = userUI.iconRow.GetComponent<RectTransform>();
            Debug.Log($"[{username}] IconRow RectTransform:" +
                $"\n  Size: {rectTransform.sizeDelta}" +
                $"\n  Scale: {rectTransform.localScale}" +
                $"\n  Position: {rectTransform.localPosition}" +
                $"\n  Pivot: {rectTransform.pivot}");

            var hlg = userUI.iconRow.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                Debug.Log($"[{username}] HorizontalLayoutGroup properties:" +
                    $"\n  Spacing: {hlg.spacing}" +
                    $"\n  Padding: L{hlg.padding.left} R{hlg.padding.right} T{hlg.padding.top} B{hlg.padding.bottom}" +
                    $"\n  Child Alignment: {hlg.childAlignment}");
            }

            CheckIconVisuals(userUI.iconHeal, "Heal");
            CheckIconVisuals(userUI.iconBleed, "Bleed");
            CheckIconVisuals(userUI.iconStun, "Stun");
        }
    }

    private void SetupUI()
    {
        Debug.Log($"[{username}] Starting SetupUI");
        
        if (uiCanvasPrefab == null)
        {
            Debug.LogError($"[{username}] uiCanvasPrefab is not assigned in the Unity Inspector!");
            return;
        }

        if (userUI == null)
        {
            Debug.LogError($"[{username}] userUI is null. Make sure the UserUI component is attached to the uiCanvasPrefab!");
            return;
        }

        try
        {
            if (userUI.nameText != null)
            {
                userUI.nameText.text = username;
                Debug.Log($"[{username}] Username set");
            }
            else
            {
                Debug.LogWarning($"[{username}] nameText is null in UserUI");
            }

            if (userUI.healthBar != null)
            {
                userUI.healthBar.interactable = false;
                userUI.healthBar.maxValue = maxHealth;
                userUI.healthBar.value = health;
                userUI.healthBar.gameObject.SetActive(true);
                UpdateHealthBarColor();
                Debug.Log($"[{username}] Health bar setup complete");
            }
            else
            {
                Debug.LogWarning($"[{username}] healthBar is null in UserUI");
            }

            if (userUI.iconRow != null)
            {
                userUI.iconRow.SetActive(true);
                if (userUI.iconHeal != null) userUI.iconHeal.SetActive(false);
                if (userUI.iconBleed != null) userUI.iconBleed.SetActive(false);
                if (userUI.iconStun != null) userUI.iconStun.SetActive(false);
                Debug.Log($"[{username}] Icon row setup complete. Position: {userUI.iconRow.transform.localPosition}");
            }
            else
            {
                Debug.LogError($"[{username}] iconRow is null in UserUI - Icons won't be displayed!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{username}] Error in SetupUI: {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateHealthBarColor()
    {
        if (userUI == null || userUI.healthBarFill == null) return;

        float percentage = (float)health / maxHealth;
        Color newColor;

        if (percentage > 0.6f)
            newColor = Color.green;
        else if (percentage > 0.3f)
            newColor = Color.yellow;
        else
            newColor = Color.red;

        StartCoroutine(LerpHealthBarColor(newColor));
    }

    private IEnumerator LerpHealthBarColor(Color targetColor)
    {
        if (userUI == null || userUI.healthBarFill == null) yield break;

        Color startColor = userUI.healthBarFill.color;
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            userUI.healthBarFill.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            yield return null;
        }

        userUI.healthBarFill.color = targetColor;
    }

    private void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        Debug.Log($"[{username}] Health updated: {health}/{maxHealth}");
    }

    private void PlayAnimation(string trigger)
    {
        if (animator == null) return;
        if (currAnimTrig == trigger) return;

        ResetAllTriggers();
        animator.SetTrigger(trigger);
        currAnimTrig = trigger;
        Debug.Log($"[{username}] Playing animation: {trigger}");
    }

    private void ResetAllTriggers()
    {
        if (animator == null) return;
        foreach (var param in animator.parameters)
            if (param.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(param.name);
        currAnimTrig = "";
    }
}
