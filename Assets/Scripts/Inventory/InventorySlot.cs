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
    /// The item currently stored in the slot.
    /// </summary>
    public InventoryItemUI CurrentItemUI => currentItemUI;

    private void Awake()
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

    public void SetCurrentItemUI(InventoryItemUI itemUI)
    {
        currentItemUI = itemUI;
    }

    /// <summary>
    /// Handles an item being dropped into this slot: validates it, updates equipment and swaps the UI.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null ||
            !eventData.pointerDrag.TryGetComponent(out InventoryItemUI draggedItem))
        {
            return;
        }

        if (allowedType != ItemType.Generic && draggedItem.Item.itemType != allowedType)
        {
            Debug.Log($"Item type {draggedItem.Item.itemType} does not match slot type {allowedType}.", this);
            return;
        }

        // Validate before touching equipment so a rejected drop changes nothing.
        if (!CanEquip(draggedItem))
        {
            return;
        }

        bool unequipped = TryUnequipFromOriginalSlot(draggedItem);
        Equip(draggedItem);
        PlaceItem(draggedItem, unequipped);
    }

    private bool CanEquip(InventoryItemUI draggedItem)
    {
        if (allowedType == ItemType.Armor && draggedItem.Item.itemType == ItemType.Armor)
        {
            Armor armor = draggedItem.Item.armorData;
            return player != null && armor != null && (!enforceArmorType || armor.type == allowedArmorType);
        }

        if (allowedType == ItemType.Weapon && draggedItem.Item.itemType == ItemType.Weapon)
        {
            return player != null && draggedItem.Item.weaponData != null;
        }

        return true; // Non-equippable slot
    }

    private void Equip(InventoryItemUI draggedItem)
    {
        if (allowedType == ItemType.Armor && draggedItem.Item.itemType == ItemType.Armor)
        {
            player.EquipArmor(draggedItem.Item.armorData);
        }
        else if (allowedType == ItemType.Weapon && draggedItem.Item.itemType == ItemType.Weapon)
        {
            player.EquipWeapon(draggedItem.Item.weaponData);
        }
    }

    /// <summary>
    /// When an equipped item leaves an equipment slot for a non-equipment slot, takes it off the player.
    /// </summary>
    private bool TryUnequipFromOriginalSlot(InventoryItemUI draggedItem)
    {
        InventorySlot origin = draggedItem.OriginalSlot;
        if (origin == null || player == null)
        {
            return false;
        }

        if (origin.allowedType == ItemType.Armor && allowedType != ItemType.Armor && draggedItem.Item.armorData != null)
        {
            player.UnequipArmor(draggedItem.Item.armorData.type);
        }
        else if (origin.allowedType == ItemType.Weapon && allowedType != ItemType.Weapon)
        {
            player.UnequipWeapon();
        }
        else
        {
            return false;
        }

        origin.SetCurrentItemUI(null);
        return true;
    }

    /// <summary>
    /// Moves the dragged item into this slot, sending any item already here back to the dragged item's origin.
    /// </summary>
    private void PlaceItem(InventoryItemUI draggedItem, bool originAlreadyCleared)
    {
        InventorySlot origin = draggedItem.OriginalSlot;

        if (!originAlreadyCleared && currentItemUI != null && origin != null)
        {
            currentItemUI.transform.SetParent(origin.transform);
            origin.SetCurrentItemUI(currentItemUI);
            UIUtils.ResetAnchoredPosition(currentItemUI.transform);
        }

        draggedItem.transform.SetParent(transform);
        SetCurrentItemUI(draggedItem);
        UIUtils.ResetAnchoredPosition(draggedItem.transform);
    }
}
