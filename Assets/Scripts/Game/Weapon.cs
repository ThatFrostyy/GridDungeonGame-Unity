using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public int damage = 50;
    public AudioClip attackSound;
    public GameObject prefab;
}