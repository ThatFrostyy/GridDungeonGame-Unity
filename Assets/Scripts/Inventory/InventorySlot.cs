using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private ItemType allowedType = ItemType.Generic;

    private InventoryItemUI currentItemUI;
    public InventoryItemUI CurrentItemUI => currentItemUI;

    public void SetCurrentItemUI(InventoryItemUI itemUI)
    {
        currentItemUI = itemUI;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<InventoryItemUI>() : null;
        if (draggedItem == null)
        {
            Debug.LogWarning("Dropped item is null or not an InventoryItemUI.", this);
            return;
        }

        // If dragging from weapon slot to a non-weapon slot, unequip
        if (draggedItem.OriginalSlot != null &&
            draggedItem.OriginalSlot.allowedType == ItemType.Weapon &&
            allowedType != ItemType.Weapon &&
            draggedItem.Item.itemType == ItemType.Weapon)
        {
            var player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                player.UnequipWeapon();
            }
        }

        if (allowedType == ItemType.Weapon && draggedItem.Item.itemType == ItemType.Weapon)
        {
            var weapon = draggedItem.Item.weaponData;
            if (weapon != null)
            {
                FindFirstObjectByType<Player>().EquipWeapon(weapon);
            }
        }

        if (allowedType == ItemType.Generic || draggedItem.Item.itemType == allowedType)
        {
            if (currentItemUI != null)
            {
                var parentSlot = draggedItem.OriginalSlot;
                if (parentSlot != null)
                {
                    currentItemUI.transform.SetParent(parentSlot.transform);
                    parentSlot.SetCurrentItemUI(currentItemUI);

                    var rect = currentItemUI.transform as RectTransform;
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero;
                    }
                }
            }

            draggedItem.transform.SetParent(transform);
            SetCurrentItemUI(draggedItem);

            var draggedRect = draggedItem.transform as RectTransform;
            if (draggedRect != null)
            {
                draggedRect.anchoredPosition = Vector2.zero;
            }
        }
    }

    public void ClearSlot()
    {
        if (allowedType == ItemType.Weapon)
        {
            var player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                player.UnequipWeapon();
            }
        }
        currentItemUI = null;
    }
}