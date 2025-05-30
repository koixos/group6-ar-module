using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class IconSetup : MonoBehaviour
{
    public Sprite iconSprite;
    public Color iconColor = Color.white;
    public float iconSize = 40f;
    public float backgroundSize = 30f;
    public float fontSize = 24f;
    
    private TextMeshProUGUI _infoText;
    public TextMeshProUGUI InfoText => _infoText;

    void OnValidate()
    {
        SetupIcon();
    }

    void SetupIcon()
    {
        // Setup main icon
        var iconObj = transform.Find("Icon")?.gameObject;
        if (iconObj == null)
        {
            iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform, false);
        }

        var iconImage = iconObj.GetComponent<Image>();
        if (iconImage == null) iconImage = iconObj.AddComponent<Image>();
        
        iconImage.sprite = iconSprite;
        iconImage.color = iconColor;
        iconImage.preserveAspect = true;

        var iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        iconRect.anchoredPosition = Vector2.zero;

        // Setup info container (position it at top-right of icon)
        var infoObj = transform.Find("Info")?.gameObject;
        if (infoObj == null)
        {
            infoObj = new GameObject("Info");
            infoObj.transform.SetParent(transform, false);
        }

        var infoRect = infoObj.GetComponent<RectTransform>();
        if (infoRect == null) infoRect = infoObj.AddComponent<RectTransform>();
        
        // Position Info at top-right corner of the icon
        infoRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
        infoRect.anchorMin = new Vector2(1, 1);  // Top-right anchor
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.pivot = new Vector2(0, 0);      // Pivot at bottom-left of Info
        infoRect.anchoredPosition = new Vector2(-5, -5);  // Slight offset from corner

        // Setup background circle
        var bgObj = infoObj.transform.Find("Background")?.gameObject;
        if (bgObj == null)
        {
            bgObj = new GameObject("Background");
            bgObj.transform.SetParent(infoObj.transform, false);
        }

        var bgImage = bgObj.GetComponent<Image>();
        if (bgImage == null) bgImage = bgObj.AddComponent<Image>();
        
        bgImage.color = new Color(1f, 1f, 1f, 0.8f);
        bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Setup text
        var textObj = infoObj.transform.Find("Text")?.gameObject;
        if (textObj == null)
        {
            textObj = new GameObject("Text");
            textObj.transform.SetParent(infoObj.transform, false);
        }

        _infoText = textObj.GetComponent<TextMeshProUGUI>();
        if (_infoText == null) _infoText = textObj.AddComponent<TextMeshProUGUI>();
        
        _infoText.fontSize = fontSize;
        _infoText.alignment = TextAlignmentOptions.Center;
        _infoText.color = Color.black;
        _infoText.text = "";  // Clear default text

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Ensure this icon has correct RectTransform
        var myRect = GetComponent<RectTransform>();
        myRect.sizeDelta = new Vector2(80f, 80f);

        // Set Info object inactive by default
        infoObj.SetActive(false);
    }

    public void ShowInfo(string text)
    {
        if (_infoText != null)
        {
            _infoText.text = text;
            _infoText.transform.parent.gameObject.SetActive(true);
        }
    }

    public void HideInfo()
    {
        if (_infoText != null)
        {
            _infoText.transform.parent.gameObject.SetActive(false);
        }
    }
} 