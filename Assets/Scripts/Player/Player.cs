using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main player component that handles combat and equipment.
/// </summary>
public class Player : ActionPointsComponent
{
    [Header("Combat")]
    [Tooltip("Null = unarmed ")]
    [SerializeField] private Weapon equippedWeapon;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private int unarmedDamage = 20;
    [Tooltip("Range for unarmed attacks")]
    [SerializeField] private int playerAttackRange = 1;

    [Header("Armor Settings")]
    [SerializeField] private Transform helmetHolder;
    [SerializeField] private Transform chestplateHolder;
    [SerializeField] private Transform shieldHolder;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip unarmedAttackSound;

    private GameObject equippedWeaponObject;
    private readonly Dictionary<Armor.ArmorType, Armor> equippedArmor = new();
    private readonly Dictionary<Armor.ArmorType, GameObject> equippedArmorObjects = new();

    public Dictionary<Armor.ArmorType, Armor> EquippedArmor => equippedArmor;

    private int EffectiveAttackRange => equippedWeapon != null ? equippedWeapon.attackRange : playerAttackRange;

    protected override void Awake()
    {
        base.Awake();
        OnActionPointsChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        OnActionPointsChanged -= UpdateUI;
    }

    private void UpdateUI(int current, int max)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAP(current, max);
        }
    }

    #region Combat

    /// <summary>
    /// Attacks an enemy if it is within range and the player has an action point.
    /// </summary>
    public void Attack(EnemyHealth enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("Attempted to attack null enemy!", this);
            return;
        }

        Vector2Int playerGrid = GridUtils.WorldToGrid(transform.position);
        Vector2Int enemyGrid = GridUtils.WorldToGrid(enemy.transform.position);

        if (GridUtils.ManhattanDistance(playerGrid, enemyGrid) > EffectiveAttackRange)
        {
            Debug.Log("Enemy is out of attack range.");
            return;
        }

        if (!UseActionPoint())
        {
            Debug.Log("Not enough Action Points!");
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger(AnimatorParams.Attack);
        }

        AudioClip attackClip = equippedWeapon != null ? equippedWeapon.attackSound : unarmedAttackSound;
        AudioUtils.PlayOneShot(audioSource, attackClip, randomizePitch: true);

        enemy.TakeDamage(equippedWeapon != null ? equippedWeapon.damage : unarmedDamage);
    }

    #endregion

    #region Weapon

    /// <summary>
    /// Equips a weapon, replacing the current one. Null unequips.
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
        if (equippedWeaponObject != null)
        {
            Destroy(equippedWeaponObject);
            equippedWeaponObject = null;
        }

        if (weapon != null && weapon.prefab != null)
        {
            equippedWeaponObject = Instantiate(weapon.prefab, weaponHolder);
            equippedWeaponObject.name = weapon.weaponName;
        }

        equippedWeapon = weapon;

        if (animator != null)
        {
            int weaponType = (int)(weapon != null ? weapon.type : Weapon.WeaponType.None);
            animator.SetInteger(AnimatorParams.WeaponType, weaponType);
        }
    }

    public void UnequipWeapon()
    {
        EquipWeapon(null);
    }

    #endregion

    #region Armor

    /// <summary>
    /// Equips armor into its slot, replacing whatever was there.
    /// </summary>
    public void EquipArmor(Armor armor)
    {
        if (armor == null)
        {
            return;
        }

        Armor.ArmorType slot = armor.type;
        DestroyArmorVisual(slot);
        equippedArmor[slot] = armor;

        Transform parent = GetArmorHolder(slot);
        if (parent != null && armor.prefab != null)
        {
            GameObject armorObject = Instantiate(armor.prefab, parent);
            armorObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            equippedArmorObjects[slot] = armorObject;
        }
    }

    public void UnequipArmor(Armor.ArmorType type)
    {
        equippedArmor.Remove(type);
        DestroyArmorVisual(type);
    }

    private void DestroyArmorVisual(Armor.ArmorType type)
    {
        if (equippedArmorObjects.Remove(type, out GameObject visual))
        {
            Destroy(visual);
        }
    }

    private Transform GetArmorHolder(Armor.ArmorType type)
    {
        return type switch
        {
            Armor.ArmorType.Helmet => helmetHolder,
            Armor.ArmorType.Chestplate => chestplateHolder,
            Armor.ArmorType.Shield => shieldHolder,
            _ => transform
        };
    }

    #endregion
}
