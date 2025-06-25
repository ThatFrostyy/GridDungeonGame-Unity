using UnityEngine;

/// <summary>
/// Handles enemy health, extends the base Health functionality
/// </summary>
public class EnemyHealth : Health
{
    [Header("Enemy Health")]
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField] private GameObject deathEffectPrefab;

    /// <summary>
    /// Handles enemy death
    /// </summary>
    protected override void Die()
    {
        RaiseOnDeath();
        SpawnDeathEffect();
        
        // Destroys the object after a small delay to allow effects
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// Spawns the death effect visual
    /// </summary>
    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}