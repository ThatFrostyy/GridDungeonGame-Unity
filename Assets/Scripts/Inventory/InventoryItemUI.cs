using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles UI interactions for individual inventory items including dragging behavior.
/// </summary>
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private InventoryItem item;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image image;
    [SerializeField] private Transform originalParent;

    /// <summary>
    /// Gets the inventory item data this UI element represents.
    /// </summary>
    public InventoryItem Item => item;

    /// <summary>
    /// Gets the original slot the item was in before dragging.
    /// </summary>
    public InventorySlot OriginalSlot { get; private set; }

    /// <summary>
    /// Initializes the item UI with given item data.
    /// </summary>
    /// <param name="itemData">Inventory item data.</param>
    public void Setup(InventoryItem itemData)
    {
        item = itemData;
        image.sprite = item.icon;
        image.preserveAspect = true;
    }

    /// <summary>
    /// Called when drag operation starts.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalSlot = transform.parent.GetComponent<InventorySlot>();
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
    }

    /// <summary>
    /// Called while dragging the item.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    /// <summary>
    /// Called when the drag operation ends.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == transform.root)
        {
            if (OriginalSlot == null) return;

            OriginalSlot.ClearSlot();
            ReturnToOriginalParent();
        }
        else
        {
            ResetRectTransform();
        }
    }

    private void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent);
        ResetRectTransform();
    }

    private void ResetRectTransform()
    {
        if (transform is RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
