using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Settings")]
    public string weaponName;
    public WeaponType type;
    public int damage = 50;
    public int attackRange;
    public GameObject prefab;

    [Header("Cosmetic")]
    public AudioClip attackSound;

    public enum WeaponType
    {
        None,
        Sword,
        Spear,
        Axe
    }
}