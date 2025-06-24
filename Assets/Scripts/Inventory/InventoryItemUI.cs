using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private InventoryItem item;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image image;
    [SerializeField] private Transform originalParent;

    public InventoryItem Item => item;
    public InventorySlot OriginalSlot { get; private set; }

    public void Setup(InventoryItem itemData)
    {
        item = itemData;
        image.sprite = item.icon;
        image.preserveAspect = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalSlot = transform.parent.GetComponent<InventorySlot>();
        originalParent = transform.parent;
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

        if (transform.parent == transform.root)
        {
            if (OriginalSlot != null)
            {
                OriginalSlot.ClearSlot();
            }
            transform.SetParent(originalParent);
            var rect = transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
        }
    }
}