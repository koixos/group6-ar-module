using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string PlayerId => playerId;

    [SerializeField] private GameObject modelParent;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Animator animator;

    private string playerId;
    private string playerName;
    private string currModel;
    private int health;
    private int maxHealth;

    public void Initialize(string id, string name, string model, int startHealth, Vector3 pos)
    {
        playerId = id;
        playerName = name;
        health = startHealth;
        maxHealth = startHealth;

        transform.position = pos;
        
        if (nameText != null)
            nameText.text = playerName;

        SetModel(model);
        UpdateHealthBar();
    }

    public void SetModel(string modelName)
    {
        if (currModel == modelName)
            return;

        currModel = modelName;

        animator = GetComponentInChildren<Animator>();
    }

    public void UpdateHealth(int _health)
    {
        health = _health;
        UpdateHealthBar();

        if (health <= 0)
        {
            PlayDefeatAnimation();
            //Destroy(gameObject, 2f);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0)
            health = 0;
        
        UpdateHealthBar();
        PlayHitAnimation();

        if (health <= 0)
            PlayDefeatAnimation();
    }

    public void PlayAttackAnimation(string attackName)
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
            animator.SetFloat("AttackType", GetAttackTypeValue(attackName));
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

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            float healthPercentage = (float)health / maxHealth;
            healthBar.transform.localScale = new Vector3(healthPercentage, 1, 1);
        }
    }

    private float GetAttackTypeValue(string attackName)
    {
        return attackName.ToLower() switch
        {
            "Attack1" => 0.1f,
            "Attack2" => 0.2f,
            "Attack3" => 0.3f,
            _ => 0f,
        };
    }
}
