using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Armor")]
public class Armor : ScriptableObject
{
    [Header("Armor Settings")]
    public string armorName;
    public ArmorType type;
    public int protection = 25;
    public GameObject prefab;

    [Header("Cosmetic")]
    public AudioClip defenseSound;

    public enum ArmorType
    {
        Helmet,
        Chestplate,
        Shield
    }
}
