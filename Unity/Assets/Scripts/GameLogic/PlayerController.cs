using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject healthBar;

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
        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.gameObject;
        //if (highlight == null)
            //highlight = transform.Find("Highlight")?.gameObject;
    }

    void LateUpdate()
    {
        if (nameText != null)
            nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - Camera.main.transform.position);
        if (healthBar != null)
            healthBar.transform.rotation = Quaternion.LookRotation(healthBar.transform.position - Camera.main.transform.position);
    }

    public void Initialize(string id, string username, string modelName, int health, int maxhealth)
    {
        this.id = id;
        this.username = username;
        this.avatar = modelName;
        this.health = health;
        this.maxHealth = maxhealth;
        
        if (nameText != null) nameText.text = username;
        
        SetHealth(health);
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
        SetHealth(health-damage);
        PlayAnimation("hurt");
        //ShowDamageAmount(damage);
    }

    public void PlayIdleAnimation() => PlayAnimation("idle");

    public void PlayDefeatAnimation() => PlayAnimation("defeat");

    public void PlayVictoryAnimation() => PlayAnimation("victory");

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
        animator.ResetTrigger("idle");
        animator.ResetTrigger("attack");
        animator.ResetTrigger("hurt");
        animator.ResetTrigger("victory");
        animator.ResetTrigger("defeat");
        currAnimTrig = "";
    }

    private void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);

        if (healthBar == null) return;

        float percentage = (float)health / maxHealth;
        if (!healthBar.TryGetComponent<SpriteRenderer>(out var sr)) return;

        if (percentage > 0.5f) sr.color = Color.green;
        else if (percentage > 0.2f) sr.color = Color.yellow;
        else sr.color = Color.red;

        healthBar.transform.localScale = new Vector3(percentage, 1, 1);
    }

    /*private void ShowDamageAmount(int damage)
    {
        // Implement damage number display logic here
        Debug.Log($"Damage: {damage}");
    }*/
}
