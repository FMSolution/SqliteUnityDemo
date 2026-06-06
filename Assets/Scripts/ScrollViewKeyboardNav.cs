using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keyboard navigation (Up/Down arrows) for a ScrollView containing Toggles.
/// - Mouse clicks on any Toggle are automatically tracked as the current selection.
/// - Arrow keys move selection to the previous/next Toggle.
/// - Content.anchoredPosition.y is directly adjusted so the selected item is always visible.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollViewKeyboardNav : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform content;
    private RectTransform viewport;

    private List<Toggle> toggles = new List<Toggle>();
    private int currentIndex = 0;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content    = scrollRect.content;
        viewport   = scrollRect.viewport;
    }

    private void Start()
    {
        RefreshToggles();
    }

    private void RefreshToggles()
    {
        foreach (Toggle t in toggles)
            if (t != null) t.onValueChanged.RemoveAllListeners();

        toggles.Clear();

        foreach (Transform child in content)
        {
            Toggle t = child.GetComponent<Toggle>();
            if (t == null) continue;

            int capturedIndex = toggles.Count;
            toggles.Add(t);

            t.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) currentIndex = capturedIndex;
            });
        }

        // Sync currentIndex with whichever toggle is already on
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn) { currentIndex = i; break; }
        }
    }

    private void Update()
    {
        if (toggles.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            Navigate(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            Navigate(-1);
    }

    private void Navigate(int direction)
    {
        int newIndex = Mathf.Clamp(currentIndex + direction, 0, toggles.Count - 1);
        if (newIndex == currentIndex) return;

        currentIndex = newIndex;
        toggles[currentIndex].isOn = true;

        EnsureVisible(currentIndex);
    }

    /// <summary>
    /// Directly sets content.anchoredPosition.y so that the item at itemIndex
    /// is fully visible inside the viewport.
    ///
    /// Layout (content pivot = top-left, anchorMin/Max = top):
    ///   - content.anchoredPosition.y == 0        → scrolled all the way to the TOP
    ///   - content.anchoredPosition.y == scrollMax → scrolled all the way to the BOTTOM
    ///
    /// Each item occupies [itemTop .. itemBottom] in content-local space (Y grows downward):
    ///   itemTop    = itemIndex * itemHeight              (distance from content top to item top edge)
    ///   itemBottom = itemTop + itemHeight
    ///
    /// The visible window in content-local space is:
    ///   windowTop    = content.anchoredPosition.y
    ///   windowBottom = content.anchoredPosition.y + viewportHeight
    /// </summary>
    private void EnsureVisible(int itemIndex)
    {
        Canvas.ForceUpdateCanvases();

        RectTransform itemRect = toggles[itemIndex].GetComponent<RectTransform>();

        float viewportHeight = viewport.rect.height;
        float contentHeight  = content.rect.height;
        float scrollMax      = Mathf.Max(0f, contentHeight - viewportHeight);

        // Item position in content-local space (Y axis points DOWN from content top)
        // anchoredPosition of the item is relative to its anchor (top of content).
        // The item's top edge = -itemRect.anchoredPosition.y - itemRect.rect.height * itemRect.pivot.y
        // The item's bottom edge = itemTop + itemRect.rect.height
        float itemTop    = -itemRect.anchoredPosition.y - itemRect.rect.height * itemRect.pivot.y;
        float itemBottom = itemTop + itemRect.rect.height;

        // Current scroll position (how far down the content has been pushed)
        float scrollY = content.anchoredPosition.y;

        // Visible window in content-local space
        float windowTop    = scrollY;
        float windowBottom = scrollY + viewportHeight;

        float newScrollY = scrollY;

        if (itemTop < windowTop)
        {
            // Item is above the visible window → scroll up so item top aligns with window top
            newScrollY = itemTop;
        }
        else if (itemBottom > windowBottom)
        {
            // Item is below the visible window → scroll down so item bottom aligns with window bottom
            newScrollY = itemBottom - viewportHeight;
        }

        newScrollY = Mathf.Clamp(newScrollY, 0f, scrollMax);

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, newScrollY);
    }
}
