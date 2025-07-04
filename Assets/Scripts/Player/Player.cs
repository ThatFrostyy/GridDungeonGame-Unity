using System.Collections.Generic;
using UnityEngine;
using static Weapon;

/// <summary>
/// Main player component that handles combat and equipment
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
    private Dictionary<Armor.ArmorType, Armor> equippedArmor = new();
    private Dictionary<Armor.ArmorType, GameObject> equippedArmorObjectMap = new();

    public Dictionary<Armor.ArmorType, Armor> EquippedArmor => equippedArmor;

    /// <summary>
    /// Initializes action points and subscribes to events
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SubscribeToEvents();
    }

    /// <summary>
    /// Subscribes to necessary events
    /// </summary>
    private void SubscribeToEvents()
    {
        OnActionPointsChanged += UpdateUI;
    }

    /// <summary>
    /// Updates UI when action points change
    /// </summary>
    private void UpdateUI(int current, int max)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAP(current, max);
        }
    }

    /// <summary>
    /// Called when action points are reset (turn start)
    /// </summary>
    protected override void OnActionPointsReset()
    {
        base.OnActionPointsReset();
        UpdateUI(currentActionPoints, maxActionPoints);
    }

    /// <summary>
    /// Executes an attack against an enemy
    /// </summary>
    /// <param name="enemy">Target enemy for the attack</param>
    public void Attack(EnemyHealth enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("Attempted to attack null enemy!", this);
            return;
        }

        Vector2Int playerGrid = GridUtils.WorldToGrid(transform.position);
        Vector2Int enemyGrid = GridUtils.WorldToGrid(enemy.transform.position);

        int manhattanDistance = Mathf.Abs(playerGrid.x - enemyGrid.x) + Mathf.Abs(playerGrid.y - enemyGrid.y);
        int effectiveRange = equippedWeapon != null ? equippedWeapon.attackRange : playerAttackRange;

        if (manhattanDistance <= effectiveRange)
        {
            if (UseActionPoint())
            {
                int damage = CalculateAttackDamage();
                AudioClip attackClip = GetAttackSound();

                PlayAttackAnimation();
                PlayAttackSound(attackClip);
                ApplyDamageToEnemy(enemy, damage);
            }
            else
            {
                Debug.Log("Not enough Action Points!");
            }
        }
        else
        {
            Debug.Log("Enemy is out of attack range.");
        }
    }

    /// <summary>
    /// Calculates attack damage based on equipped weapon
    /// </summary>
    private int CalculateAttackDamage()
    {
        return equippedWeapon ? equippedWeapon.damage : unarmedDamage;
    }

    /// <summary>
    /// Gets attack sound based on equipped weapon
    /// </summary>
    private AudioClip GetAttackSound()
    {
        return equippedWeapon ? equippedWeapon.attackSound : unarmedAttackSound;
    }

    /// <summary>
    /// Plays the attack animation
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// Plays attack sound with pitch variation
    /// </summary>
    private void PlayAttackSound(AudioClip attackClip)
    {
        if (audioSource != null && attackClip != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(attackClip);
            audioSource.pitch = 1f; // Reset pitch
        }
    }

    /// <summary>
    /// Applies damage to the target enemy
    /// </summary>
    private void ApplyDamageToEnemy(EnemyHealth enemy, int damage)
    {
        enemy.TakeDamage(damage);
    }

    /// <summary>
    /// Equips a new weapon, destroying the previous one if it exists
    /// </summary>
    /// <param name="weapon">Weapon to equip (null to unequip)</param>
    public void EquipWeapon(Weapon weapon)
    {
        UnequipCurrentWeapon();

        if (weapon != null && weapon.prefab != null)
        {
            CreateWeaponInstance(weapon);
        }

        equippedWeapon = weapon;
        UpdateAnimatorWeaponType(weapon);
    }

    /// <summary>
    /// Unequips the current weapon
    /// </summary>
    public void UnequipWeapon()
    {
        EquipWeapon(null);
    }

    /// <summary>
    /// Destroys the current weapon instance
    /// </summary>
    private void UnequipCurrentWeapon()
    {
        if (equippedWeaponObject != null)
        {
            Destroy(equippedWeaponObject);
            equippedWeaponObject = null;
        }
    }

    public void EquipArmor(Armor armor)
    {
        if (armor == null) return;

        Armor.ArmorType slot = armor.type;

        // Replace existing armor in this slot
        if (equippedArmor.ContainsKey(slot))
        {
            equippedArmor[slot] = armor;
        }
        else
        {
            equippedArmor.Add(slot, armor);
        }

        // Visual instantiation
        Transform parent = GetArmorHolder(slot);
        if (parent != null && armor.prefab != null)
        {
            GameObject armorObj = Instantiate(armor.prefab, parent);
            armorObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            equippedArmorObjectMap[slot] = armorObj;
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

    public void UnequipArmor(Armor.ArmorType type)
    {
        if (equippedArmor.ContainsKey(type))
        {
            equippedArmor.Remove(type);

            if (equippedArmorObjectMap.ContainsKey(type))
            {
                Destroy(equippedArmorObjectMap[type]);
                equippedArmorObjectMap.Remove(type);
            }
        }
    }

    /// <summary>
    /// Creates the visual weapon instance
    /// </summary>
    private void CreateWeaponInstance(Weapon weapon)
    {
        equippedWeaponObject = Instantiate(weapon.prefab, weaponHolder);
        equippedWeaponObject.name = weapon.weaponName;
    }

    /// <summary>
    /// Updates the weapon type parameter in the animator
    /// </summary>
    private void UpdateAnimatorWeaponType(Weapon weapon)
    {
        if (animator != null)
        {
            int weaponType = (int)(weapon != null ? weapon.type : WeaponType.None);
            animator.SetInteger("WeaponType", weaponType);
        }
    }
}