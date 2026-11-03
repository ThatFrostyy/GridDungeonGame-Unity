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
    /// The inventory item data this UI element represents.
    /// </summary>
    public InventoryItem Item => item;

    /// <summary>
    /// The slot the item was in before dragging.
    /// </summary>
    public InventorySlot OriginalSlot { get; private set; }

    public void Setup(InventoryItem itemData)
    {
        item = itemData;
        image.sprite = item.icon;
        image.preserveAspect = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        OriginalSlot = originalParent.GetComponent<InventorySlot>();
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Still parented to the root means no slot accepted the drop: snap back where it came from.
        // Equipment state is untouched, so the item stays equipped if it was.
        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
        }

        UIUtils.ResetAnchoredPosition(transform);
    }
}
