using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class IconSetup : MonoBehaviour
{
    [Header("Icon Settings")]
    public Sprite iconSprite;
    public Color iconColor = Color.white;
    public float iconSize = 80f;

    [Header("Info Badge Settings")]
    public float backgroundSize = 40f;
    public float fontSize = 20f;
    public Color backgroundColor = new Color(1f, 1f, 1f, 0.9f);
    public Color textColor = Color.black;

    private TextMeshProUGUI _infoText;
    private GameObject _infoObject;

    public TextMeshProUGUI InfoText => _infoText;

    void Start()
    {
        if (Application.isPlaying)
        {
            SetupIcon();
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            SetupIcon();
        }
    }

    void SetupIcon()
    {
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
        iconImage.raycastTarget = false; // Performance için

        var iconRect = iconObj.GetComponent<RectTransform>();
        if (iconRect == null) iconRect = iconObj.AddComponent<RectTransform>();

        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;

        _infoObject = transform.Find("Info")?.gameObject;
        if (_infoObject == null)
        {
            _infoObject = new GameObject("Info");
            _infoObject.transform.SetParent(transform, false);
        }

        var infoRect = _infoObject.GetComponent<RectTransform>();
        if (infoRect == null) infoRect = _infoObject.AddComponent<RectTransform>();

        // Info badge'i sað üst köþeye yerleþtir
        infoRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
        infoRect.anchorMin = new Vector2(1f, 1f);
        infoRect.anchorMax = new Vector2(1f, 1f);
        infoRect.pivot = new Vector2(0.5f, 0.5f);
        infoRect.anchoredPosition = new Vector2(-backgroundSize * 0.3f, -backgroundSize * 0.3f);

        // Background circle
        SetupBackground();

        // Text
        SetupText();

        // Baþlangýçta gizli
        _infoObject.SetActive(false);

        var myRect = GetComponent<RectTransform>();
        if (myRect == null) myRect = gameObject.AddComponent<RectTransform>();

        myRect.sizeDelta = new Vector2(iconSize, iconSize);

        // Layout Group için gerekli component'ler
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = gameObject.AddComponent<LayoutElement>();

        layoutElement.preferredWidth = iconSize;
        layoutElement.preferredHeight = iconSize;
    }

    private void SetupBackground()
    {
        var bgObj = _infoObject.transform.Find("Background")?.gameObject;
        if (bgObj == null)
        {
            bgObj = new GameObject("Background");
            bgObj.transform.SetParent(_infoObject.transform, false);
        }

        var bgImage = bgObj.GetComponent<Image>();
        if (bgImage == null) bgImage = bgObj.AddComponent<Image>();

        bgImage.color = backgroundColor;
        bgImage.raycastTarget = false;

        // Daire þekli için sprite
        bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        if (bgImage.sprite == null)
        {
            // Fallback: Varsayýlan UI sprite
            bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        var bgRect = bgObj.GetComponent<RectTransform>();
        if (bgRect == null) bgRect = bgObj.AddComponent<RectTransform>();

        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
    }

    private void SetupText()
    {
        var textObj = _infoObject.transform.Find("Text")?.gameObject;
        if (textObj == null)
        {
            textObj = new GameObject("Text");
            textObj.transform.SetParent(_infoObject.transform, false);
        }

        _infoText = textObj.GetComponent<TextMeshProUGUI>();
        if (_infoText == null) _infoText = textObj.AddComponent<TextMeshProUGUI>();

        _infoText.fontSize = fontSize;
        _infoText.alignment = TextAlignmentOptions.Center;
        _infoText.color = textColor;
        _infoText.text = "";
        _infoText.raycastTarget = false; // Performance için
        _infoText.fontStyle = FontStyles.Bold; // Daha belirgin

        // Auto size ayarlarý
        _infoText.enableAutoSizing = true;
        _infoText.fontSizeMin = fontSize * 0.5f;
        _infoText.fontSizeMax = fontSize;

        var textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null) textRect = textObj.AddComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Padding ekle
        textRect.offsetMin = new Vector2(2, 2);
        textRect.offsetMax = new Vector2(-2, -2);
    }

    public void ShowInfo(string text)
    {
        if (_infoText != null && _infoObject != null)
        {
            _infoText.text = text;
            _infoObject.SetActive(true);

            Debug.Log($"Showing info on {gameObject.name}: '{text}'");
        }
        else
        {
            Debug.LogError($"Cannot show info on {gameObject.name} - components missing!");
        }
    }

    public void HideInfo()
    {
        if (_infoObject != null)
        {
            _infoObject.SetActive(false);
            Debug.Log($"Hiding info on {gameObject.name}");
        }
    }
} 