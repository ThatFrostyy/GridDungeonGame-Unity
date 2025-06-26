using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a UI slot in the inventory that accepts and manages inventory items.
/// </summary>
public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    [SerializeField] private ItemType allowedType = ItemType.Generic;

    [Header("Armor Settings")]
    [Tooltip("ONLY used when Enforce Armor Type is true!")]
    [SerializeField] private Armor.ArmorType allowedArmorType;
    [SerializeField] private bool enforceArmorType = false;

    private InventoryItemUI currentItemUI;
    private Player player;

    /// <summary>
    /// Gets the currently stored item in the slot.
    /// </summary>
    public InventoryItemUI CurrentItemUI => currentItemUI;

    private void Awake()
    {
        InitializeReferences();
    }

    /// <summary>
    /// Gets necessary references from the GameObjectLocator.
    /// </summary>
    private void InitializeReferences()
    {
        if (GameObjectLocator.Instance != null)
        {
            player = GameObjectLocator.Instance.Player;
        }
        else
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
        }
    }

    /// <summary>
    /// Sets the current InventoryItemUI.
    /// </summary>
    /// <param name="itemUI">Item UI to assign.</param>
    public void SetCurrentItemUI(InventoryItemUI itemUI)
    {
        currentItemUI = itemUI;
    }

    /// <summary>
    /// Handles logic when an item is dropped into the slot.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();

        if (eventData.pointerDrag == null) return;
        if (draggedItem == null) return;

        if (allowedType != ItemType.Generic && draggedItem.Item.itemType != allowedType)
        {
            Debug.Log($"Item type {draggedItem.Item.itemType} does not match slot type {allowedType}.", this);
            return;
        }

        bool unequipped = TryUnequipFromOriginalSlot(draggedItem);

        if (!TryEquipToNewSlot(draggedItem)) return;

        HandleUIPlacement(draggedItem, unequipped);
    }

    private bool TryUnequipFromOriginalSlot(InventoryItemUI draggedItem)
    {
        if (draggedItem.OriginalSlot == null) return false;

        if (draggedItem.OriginalSlot.allowedType == ItemType.Armor && allowedType != ItemType.Armor)
        {
            if (player != null)
            {
                player.UnequipArmor(draggedItem.Item.armorData.type);
                draggedItem.OriginalSlot.SetCurrentItemUI(null);
                return true;
            }
        }
        else if (draggedItem.OriginalSlot.allowedType == ItemType.Weapon && allowedType != ItemType.Weapon)
        {
            if (player != null)
            {
                player.UnequipWeapon();
                draggedItem.OriginalSlot.SetCurrentItemUI(null);
                return true;
            }
        }
        return false;
    }

    private bool TryEquipToNewSlot(InventoryItemUI draggedItem)
    {
        if (allowedType == ItemType.Armor && draggedItem.Item.itemType == ItemType.Armor)
        {
            var armor = draggedItem.Item.armorData;
            if (armor == null || (enforceArmorType && armor.type != allowedArmorType))
            {
                return false;
            }

            if (player == null) return false;

            player.EquipArmor(armor);
            SetCurrentItemUI(draggedItem);
            return true;
        }

        if (allowedType == ItemType.Weapon && draggedItem.Item.itemType == ItemType.Weapon)
        {
            var weapon = draggedItem.Item.weaponData;

            if (weapon == null) return false;
            if (player == null) return false;

            player.EquipWeapon(weapon);
            SetCurrentItemUI(draggedItem);
            return true;
        }

        return true; // Non-equippable types
    }

    private void HandleUIPlacement(InventoryItemUI draggedItem, bool unequipped)
    {
        if (!unequipped && currentItemUI != null && draggedItem.OriginalSlot != null)
        {
            currentItemUI.transform.SetParent(draggedItem.OriginalSlot.transform);
            draggedItem.OriginalSlot.SetCurrentItemUI(currentItemUI);
            ResetRectTransform(currentItemUI.transform);
        }

        draggedItem.transform.SetParent(transform);
        SetCurrentItemUI(draggedItem);
        ResetRectTransform(draggedItem.transform);
    }

    private void ResetRectTransform(Transform trans)
    {
        if (trans is RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Clears the slot and unequips the weapon if applicable.
    /// </summary>
    public void ClearSlot()
    {
        if (allowedType == ItemType.Weapon)
        {
            if (player == null) return;

            player.UnequipWeapon();
        }
        currentItemUI = null;
    }
}