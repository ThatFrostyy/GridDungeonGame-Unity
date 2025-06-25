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
    public EnemyHealthBar HealthBar => healthBar;

    /// <summary>
    /// Initializes action points and sets up health bar
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetupHealthBar();
    }

    /// <summary>
    /// Checks if the enemy is alive
    /// </summary>
    public bool IsAlive => health != null && health.IsAlive;

    /// <summary>
    /// Gets the enemy's health percentage using Unity’s overloaded null check
    /// </summary>
    public float HealthPercentage
    {
        get
        {
            return health != null ? health.HealthPercentage : 0f;
        }
    }

    /// <summary>
    /// Sets up the enemy health bar if components are assigned
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
}
