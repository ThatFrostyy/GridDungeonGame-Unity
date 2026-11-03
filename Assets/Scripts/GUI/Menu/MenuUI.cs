using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small helpers that build uGUI elements from code, so the menus need no scene or prefab setup.
/// </summary>
public static class MenuUI
{
    private static readonly Color PanelColor = new(0.05f, 0.05f, 0.08f, 0.92f);
    private static readonly Color ButtonColor = new(0.16f, 0.16f, 0.2f, 1f);
    private static readonly Color AccentColor = new(1f, 0.72f, 0.1f, 1f);
    private static readonly Color TrackColor = new(0.1f, 0.1f, 0.13f, 1f);

    public static RectTransform CreateFullscreenPanel(Transform parent, string name)
    {
        RectTransform rect = CreateRect(name, parent);
        Stretch(rect);
        rect.gameObject.AddComponent<Image>().color = PanelColor;
        return rect;
    }

    /// <summary>
    /// A centered vertical stack that grows to fit its children.
    /// </summary>
    public static RectTransform CreateColumn(Transform parent, float width, float spacing = 16f)
    {
        RectTransform rect = CreateRect("Column", parent);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, 0f);

        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rect;
    }

    public static TMP_Text CreateLabel(Transform parent, string text, float size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        RectTransform rect = CreateRect("Label", parent);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;

        var element = rect.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = size * 1.5f;
        return label;
    }

    public static Button CreateButton(Transform parent, string text, Action onClick, float height = 64f)
    {
        RectTransform rect = CreateRect(text + " Button", parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = ButtonColor;

        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = ButtonColors();
        button.onClick.AddListener(() => onClick?.Invoke());

        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = height;

        TMP_Text label = CreateLabel(rect, text, 30f);
        Stretch(label.rectTransform);
        label.name = "Text";
        label.GetComponent<LayoutElement>().ignoreLayout = true;
        return button;
    }

    public static TMP_Text GetButtonLabel(Button button)
    {
        return button.GetComponentInChildren<TMP_Text>();
    }

    /// <summary>
    /// A "Label ---o---- 70%" row. onChanged receives the new 0..1 value.
    /// </summary>
    public static Slider CreateSliderRow(Transform parent, string label, float value, Action<float> onChanged)
    {
        RectTransform row = CreateRow(parent, 56f);

        TMP_Text text = CreateLabel(row, label, 28f, TextAlignmentOptions.MidlineLeft);
        text.GetComponent<LayoutElement>().preferredWidth = 260f;

        Slider slider = CreateSlider(row);
        slider.GetComponent<LayoutElement>().flexibleWidth = 1f;

        TMP_Text percent = CreateLabel(row, string.Empty, 28f, TextAlignmentOptions.MidlineRight);
        percent.GetComponent<LayoutElement>().preferredWidth = 90f;

        void Refresh(float v) => percent.text = $"{Mathf.RoundToInt(v * 100f)}%";

        slider.SetValueWithoutNotify(value);
        Refresh(value);
        slider.onValueChanged.AddListener(v =>
        {
            Refresh(v);
            onChanged?.Invoke(v);
        });

        return slider;
    }

    /// <summary>
    /// A "Label            [ Key ]" row. Returns the key button.
    /// </summary>
    public static Button CreateKeyRow(Transform parent, string label, Action onClick)
    {
        RectTransform row = CreateRect("Key Row", parent);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;

        TMP_Text text = CreateLabel(row, label, 28f, TextAlignmentOptions.MidlineLeft);
        text.GetComponent<LayoutElement>().flexibleWidth = 1f;

        Button button = CreateButton(row, string.Empty, onClick, 56f);
        button.GetComponent<LayoutElement>().preferredWidth = 280f;
        return button;
    }

    private static RectTransform CreateRow(Transform parent, float height)
    {
        RectTransform row = CreateRect("Row", parent);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
        return row;
    }

    private static Slider CreateSlider(Transform parent)
    {
        RectTransform root = CreateRect("Slider", parent);
        root.gameObject.AddComponent<LayoutElement>();

        RectTransform background = CreateRect("Background", root);
        background.anchorMin = new Vector2(0f, 0.35f);
        background.anchorMax = new Vector2(1f, 0.65f);
        background.offsetMin = background.offsetMax = Vector2.zero;
        background.gameObject.AddComponent<Image>().color = TrackColor;

        RectTransform fillArea = CreateRect("Fill Area", root);
        fillArea.anchorMin = new Vector2(0f, 0.35f);
        fillArea.anchorMax = new Vector2(1f, 0.65f);
        fillArea.offsetMin = new Vector2(0f, 0f);
        fillArea.offsetMax = new Vector2(-15f, 0f);

        RectTransform fill = CreateRect("Fill", fillArea);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.sizeDelta = new Vector2(10f, 0f);
        fill.gameObject.AddComponent<Image>().color = AccentColor;

        RectTransform handleArea = CreateRect("Handle Slide Area", root);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(15f, 0f);
        handleArea.offsetMax = new Vector2(-15f, 0f);

        RectTransform handle = CreateRect("Handle", handleArea);
        handle.anchorMin = Vector2.zero;
        handle.anchorMax = new Vector2(0f, 1f);
        handle.sizeDelta = new Vector2(30f, 0f);
        var handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = Color.white;

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        return slider;
    }

    private static ColorBlock ButtonColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.85f, 0.45f, 1f);
        colors.pressedColor = new Color(0.8f, 0.6f, 0.1f, 1f);
        colors.selectedColor = Color.white;
        return colors;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
