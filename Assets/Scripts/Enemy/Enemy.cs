using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main enemy component that handles references and configuration.
/// </summary>
public class Enemy : ActionPointsComponent
{
    private static readonly List<Enemy> ActiveEnemies = new();

    [Header("References")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyHealthBar healthBar;

    /// <summary>
    /// Every enabled enemy in the scene, including ones spawned at runtime.
    /// </summary>
    public static IReadOnlyList<Enemy> Active => ActiveEnemies;

    public EnemyHealth Health => health;
    public EnemyHealthBar HealthBar => healthBar;
    public bool IsAlive => health != null && health.IsAlive;
    public float HealthPercentage => health != null ? health.HealthPercentage : 0f;

    private void OnEnable()
    {
        ActiveEnemies.Add(this);
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    // Start (not Awake) so the health component has already initialized its values.
    private void Start()
    {
        SetupHealthBar();
    }

    private void SetupHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        if (health == null)
        {
            Debug.LogWarning($"Enemy {name}: HealthBar assigned but Health component is missing!", this);
            return;
        }

        healthBar.Setup(health, transform);
    }
}
