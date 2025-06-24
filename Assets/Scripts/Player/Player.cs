using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Action Points")]
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int currentActionPoints;

    [Header("Combat")]
    [SerializeField] private Weapon equippedWeapon; // Null = unarmed
    [SerializeField] private int unarmedDamage = 3;
    [SerializeField] private AudioClip unarmedAttackSound;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHolder;

    [Header("References")]
    [SerializeField] private RuntimeAnimatorController defaultController;

    private GameObject equippedWeaponObject;

    public int MaxActionPoints => maxActionPoints;
    public int CurrentActionPoints => currentActionPoints;

    private void Awake()
    {
        currentActionPoints = maxActionPoints;
    }

    public bool UseActionPoint(int amount = 1)
    {
        if (currentActionPoints >= amount)
        {
            currentActionPoints -= amount;
            UIManager.Instance.UpdateAP(currentActionPoints, maxActionPoints);
            return true;
        }
        return false;
    }

    public void GainActionPoints(int amount)
    {
        currentActionPoints = Mathf.Min(currentActionPoints + amount, maxActionPoints);
    }

    public void LoseActionPoints(int amount)
    {
        currentActionPoints = Mathf.Max(currentActionPoints - amount, 0);
    }

    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
        UIManager.Instance.UpdateAP(currentActionPoints, maxActionPoints);
    }

    public void Attack(EnemyHealth enemy)
    {
        int damage = equippedWeapon ? equippedWeapon.damage : unarmedDamage;
        AudioClip attackClip = equippedWeapon ? equippedWeapon.attackSound : unarmedAttackSound;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (audioSource != null && attackClip != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(attackClip);

            audioSource.pitch = 1f;
        }

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (equippedWeaponObject != null)
        {
            Destroy(equippedWeaponObject);
        }

        if (weapon != null && weapon.prefab != null)
        {
            equippedWeaponObject = Instantiate(weapon.prefab, weaponHolder);
            equippedWeaponObject.name = weapon.weaponName;
        }

        equippedWeapon = weapon;

        // Set the weapon type parameter for the animator
        if (animator != null)
        {
            animator.SetInteger("WeaponType", GetWeaponTypeInt(weapon));
        }
    }

    public void UnequipWeapon()
    {
        if (equippedWeaponObject != null)
        {
            Destroy(equippedWeaponObject);
            equippedWeaponObject = null;
        }

        equippedWeapon = null;

        // Reset to unarmed
        if (animator != null)
        {
            animator.SetInteger("WeaponType", 0);
        }
        animator.runtimeAnimatorController = defaultController;
    }

    private int GetWeaponTypeInt(Weapon weapon)
    {
        if (weapon == null) return 0; // 0 = unarmed
        if (weapon.weaponName == "Sword") return 1;

        return 0;
    }
}