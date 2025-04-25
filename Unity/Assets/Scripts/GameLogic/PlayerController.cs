using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject highlight;

    [Header("Model")]
    public string id;
    public string username;
    public string avatar;
    public int health;

    private int maxHealth;
    private Animator animator;

    public void Initialize(string id, string username, string modelName, int health)
    {
        this.id = id;
        this.username = username;
        this.avatar = modelName;
        this.health = health;
        this.maxHealth = health;
        
        if (nameText != null)
            nameText.text = username;

        SetAvatar(modelName);
        SetHealth(health);
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
        float healthPercentage = (float)health / maxHealth;
        if (healthBar != null)
            healthBar.transform.localScale = new Vector3(healthPercentage, 1, 1);

        if (health <= 0)
            PlayDefeatAnimation();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0)
            health = 0;

        PlayHitAnimation();
        SetHealth(health);
    }

    public void PlayIdleAnimation()
    {
        if (animator != null)
            animator.SetTrigger("idle");
    }

    public void PlayAttackAnimation(string attackName)
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
            //animator.SetFloat("AttackType", GetAttackTypeValue(attackName));
        }
    }

    public void PlayHitAnimation()
    {
        if (animator != null)
            animator.SetTrigger("hit");
    }

    public void PlayDefeatAnimation()
    {
        if (animator != null)
            animator.SetTrigger("defeat");
    }

    public void Highlight(bool on)
    {
        if (highlight != null)
            highlight.SetActive(on);
    }

    private void SetAvatar(string modelName)
    {
        GameObject avatarPrefab = Resources.Load<GameObject>($"Avatars/{modelName}");
        if (avatarPrefab != null)
        {
            GameObject avatarInstance = Instantiate(avatarPrefab, transform);
            animator = avatarInstance.GetComponent<Animator>();
        }
    }
}
