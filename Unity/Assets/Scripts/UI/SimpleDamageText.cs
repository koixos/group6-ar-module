using System.Collections;
using TMPro;
using UnityEngine;

public class SimpleDamageText : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private float scaleMultiplier = 0.01f; // For proper size in world space

    [SerializeField] private TextMeshProUGUI textMesh;

    public void ShowDamage(int damage, Color color)
    {
        if (textMesh == null) return;

        Debug.Log($"Showing damage text: {damage} with color: {color}");

        // Set text and color
        textMesh.text = damage.ToString();
        textMesh.color = color;

        // Start animation
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        if (!TryGetComponent<CanvasGroup>(out var canvasGroup)) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        while (elapsed < 1f)
        {
            transform.position = startPos + Vector3.up * elapsed * 0.5f;
            canvasGroup.alpha = 1f - elapsed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
