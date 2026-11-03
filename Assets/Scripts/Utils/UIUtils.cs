using UnityEngine;

public static class UIUtils
{
    /// <summary>
    /// Centers a UI element inside its parent.
    /// </summary>
    public static void ResetAnchoredPosition(Transform target)
    {
        if (target is RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
