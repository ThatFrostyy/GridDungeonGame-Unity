using UnityEngine;

/// <summary>
/// Owns the inventory slots and toggles the inventory window.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Slots")]
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private InventorySlot headSlot;
    [SerializeField] private InventorySlot bodySlot;
    [SerializeField] private InventorySlot weaponSlot;
    [SerializeField] private InventorySlot shieldSlot;
    [SerializeField] private GameObject inventoryUI;

    [Header("Prefabs")]
    public InventoryItemUI itemUIPrefab;

    [Header("TESTING")]
    [SerializeField] private bool isTesting = false;
    [SerializeField] private InventoryItem testItem;
    [SerializeField] private InventoryItem testItem1;
    [SerializeField] private InventoryItem testItem2;

    private void Awake()
    {
        if (!isTesting)
        {
            return;
        }

        AddItemToInventory(testItem);
        AddItemToInventory(testItem1);
        AddItemToInventory(testItem2);
    }

    private void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    /// <summary>
    /// Puts an item into the first free inventory slot. Returns false if there is no room.
    /// </summary>
    public bool AddItemToInventory(InventoryItem item)
    {
        if (item == null || itemUIPrefab == null)
        {
            return false;
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.CurrentItemUI != null)
            {
                continue;
            }

            InventoryItemUI itemUI = Instantiate(itemUIPrefab, slot.transform);
            itemUI.Setup(item);
            slot.SetCurrentItemUI(itemUI);
            return true;
        }

        Debug.LogWarning($"Inventory is full, could not add {item.itemName}.", this);
        return false;
    }

    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }
}
