using UnityEngine;

/// <summary>
/// Main enemy component that handles references and configuration
/// </summary>
public class Enemy : ActionPointsComponent
{
    [Header("References")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyHealthBar healthBar;

    /// <summary>
    /// Gets the reference to the health component
    /// </summary>
    public EnemyHealth Health => health;

    /// <summary>
    /// Initializes action points and sets up health bar
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetupHealthBar();
    }

    /// <summary>
    /// Sets up the health bar if components are assigned
    /// </summary>
    private void SetupHealthBar()
    {
        if (health != null && healthBar != null)
        {
            healthBar.Setup(health, transform);
        }
        else if (healthBar != null)
        {
            Debug.LogWarning($"Enemy {name}: HealthBar assigned but Health component is missing!", this);
        }
    }

    /// <summary>
    /// Checks if the enemy is alive
    /// </summary>
    public bool IsAlive => health != null && health.IsAlive;

    /// <summary>
    /// Gets the enemy's health percentage
    /// </summary>
    public float HealthPercentage => health?.HealthPercentage ?? 0f;
}
