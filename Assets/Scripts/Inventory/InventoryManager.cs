using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Slots")]
    public InventorySlot[] inventorySlots;
    public InventorySlot headSlot;
    public InventorySlot bodySlot;
    public InventorySlot weaponSlot;
    public InventorySlot shieldSlot;
    [SerializeField] private GameObject inventoryUI; 

    [Header("Prefabs")]
    public InventoryItemUI itemUIPrefab;

    [Header("TESTING")]
    [SerializeField] private bool isTesting = false;
    [SerializeField] private InventoryItem testItem;
    [SerializeField] private InventoryItem testItem1;

    private void Awake()
    {
        if (isTesting)
        {
            AddItemToInventory(testItem);
            AddItemToInventory(testItem1);
        }
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

    public void AddItemToInventory(InventoryItem item)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.CurrentItemUI == null)
            {
                var itemUI = Instantiate(itemUIPrefab, slot.transform);
                itemUI.Setup(item);

                slot.SetCurrentItemUI(itemUI);
                break;
            }
        }
    }

    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }
}