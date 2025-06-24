using UnityEngine;

/// <summary>
/// Main player component that handles combat and equipment
/// </summary>
public class Player : ActionPointsComponent
{
    [Header("Combat")]
    [SerializeField] private Weapon equippedWeapon; // Null = unarmed

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private int unarmedDamage = 3;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip unarmedAttackSound;

    private GameObject equippedWeaponObject;

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

        int damage = CalculateAttackDamage();
        AudioClip attackClip = GetAttackSound();

        PlayAttackAnimation();
        PlayAttackSound(attackClip);
        ApplyDamageToEnemy(enemy, damage);
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
            int weaponType = GetWeaponTypeInt(weapon);
            animator.SetInteger("WeaponType", weaponType);
        }
    }

    /// <summary>
    /// Converts weapon type to integer for animator
    /// </summary>
    /// <param name="weapon">Weapon to convert</param>
    /// <returns>0 = no weapon, 1 = sword, etc.</returns>
    private int GetWeaponTypeInt(Weapon weapon)
    {
        if (weapon == null) return 0; // No weapon
        
        // TODO: Use enums or dictionary for more scalable mapping
        return weapon.weaponName switch
        {
            "Sword" => 1,
            _ => 0
        };
    }
}