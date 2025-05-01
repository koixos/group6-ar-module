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
    private GameObject currentAttackEffect;

    public void Initialize(string id, string username, string modelName, int health)
    {
        this.id = id;
        this.username = username;
        this.avatar = modelName;
        this.health = health;
        this.maxHealth = health;
        
        if (nameText != null)
            nameText.text = username;

        SetHealth(health);
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
        float healthPercentage = (float)health / maxHealth;
        
        if (healthBar != null)
            healthBar.transform.localScale = new Vector3(healthPercentage, 1, 1);

        if (healthBar != null)
        {
            if (healthPercentage > 0.5f)
                healthBar.GetComponent<SpriteRenderer>().color = Color.green;
            else if (healthPercentage > 0.2f)
                healthBar.GetComponent<SpriteRenderer>().color = Color.yellow;
            else
                healthBar.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public void Highlight(bool on)
    {
        if (highlight != null)
            highlight.SetActive(on);
    }

    public void Attack(string attackName)
    {
        if (animator != null)
        {
            animator.ResetTrigger("idle");
            animator.ResetTrigger("hit");
            animator.SetTrigger("attack");
            ShowAttackAnimation(attackName);
        }
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0) return;

        health -= damage;
        if (health < 0)
            health = 0;

        PlayHitAnimation();
        SetHealth(health);
        ShowDamageNumber(damage);
    }

    public void PlayIdleAnimation()
    {
        if (animator != null)
            animator.SetTrigger("idle");
    }    

    public void PlayDefeatAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger("idle");
            animator.ResetTrigger("attack");
            animator.ResetTrigger("hit");
            animator.SetTrigger("defeat");
        }
    }

    public void PlayVictoryAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger("idle");
            animator.ResetTrigger("attack");
            animator.ResetTrigger("hit");
            animator.SetTrigger("victory");
        }
    }

    private void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger("idle");
            animator.ResetTrigger("attack");
            animator.SetTrigger("hit");
        }
    }

    private void ShowAttackAnimation(string attackName)
    {
        if (currentAttackEffect != null)
            Destroy(currentAttackEffect);

        GameObject attackPrefab = Resources.Load<GameObject>($"Attacks/{attackName}");
        if (attackPrefab != null)
        {
            Vector3 attackPosition = transform.position + transform.forward * 2f;
            currentAttackEffect = Instantiate(attackPrefab, attackPosition, transform.rotation);
            currentAttackEffect.transform.LookAt(transform.position - transform.forward * 10f);
            Destroy(currentAttackEffect, 3f);
        }
    }

    private void ShowDamageNumber(int damage)
    {
        // Implement damage number display logic here
        Debug.Log($"Damage: {damage}");
    }

    private void SetAvatar(string modelName)
    {
        Transform existingAvatar = transform.Find("Avatar");
        if (existingAvatar != null)
            Destroy(existingAvatar.gameObject);
    }
}
