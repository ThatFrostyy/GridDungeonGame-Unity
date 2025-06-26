using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Item Settings")]
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    [Header("Item Data")]
    [Tooltip("ONLY set for Armor item types!")]
    public Armor armorData;
    [Tooltip("ONLY set for Weapon item types!")]
    public Weapon weaponData;
}

public enum ItemType
{
    Generic,
    Armor,
    Weapon
}