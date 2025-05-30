using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float scaleAnimation = 1.2f;

    public void ShowDamage(int damage, Color color)
    {
        Debug.Log($"SimpleDamageText.ShowDamage called: damage={damage}, color={color}");

        if (damageText == null)
        {
            damageText = GetComponent<TextMeshProUGUI>();
            if (damageText == null)
            {
                Debug.LogError("TextMeshPro component not found!");
                return;
            }
        }

        Debug.Log($"TextMeshPro found: {damageText.name}");
        damageText.text = damage.ToString();
        damageText.color = color;
        damageText.fontSize = 8f;

        Debug.Log($"Text set to: {damageText.text}, Color: {damageText.color}");
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
            Debug.Log("Text rotated to face camera");
        }
        else
        {
            Debug.LogWarning("Main camera not found!");
        }

        Debug.Log("Starting animation coroutine");

        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        float elapsedTime = 0f;

        while (elapsedTime < lifetime)
        {
            float progress = elapsedTime / lifetime;

            transform.position = startPos + Vector3.up * (moveSpeed * elapsedTime);

            float scaleMultiplier = progress < 0.2f ?
                Mathf.Lerp(1f, scaleAnimation, progress / 0.2f) :
                Mathf.Lerp(scaleAnimation, 0.8f, (progress - 0.2f) / 0.8f);
            transform.localScale = startScale * scaleMultiplier;

            Color currentColor = damageText.color;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            damageText.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
