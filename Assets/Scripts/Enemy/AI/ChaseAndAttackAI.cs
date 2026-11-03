using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Turn-based enemy AI: gathers the inspector settings and scene references,
/// then delegates every decision to the state machine (Patrol, Retreat, Attack, Chase).
/// </summary>
[RequireComponent(typeof(EnemyStateMachine))]
public class ChaseAndAttackAI : EnemyAI
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int spotRange = 3;
    [Tooltip("Health percentage below which the enemy will retreat.")]
    [Range(0f, 1f)] [SerializeField] private float retreatHealth = 0.3f;
    [Tooltip("Tile size used to draw the spot range gizmo.")]
    [SerializeField] private Vector2 gridSize = new(1f, 1f);

    [Header("Combat Settings")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private int attackRange = 1;
    [Range(0f, 1f)] [SerializeField] private float critChance = 0.2f;
    [SerializeField] private float critMultiplier = 1.5f;
    [SerializeField] private AudioClip critSound;

    [Header("Sprite Settings")]
    [SerializeField] private Animator animator;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip attackSound;

    [Header("References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private AudioSource audioSource;

    [Header("Editor")]
    [SerializeField] private Color spotRangeColor = Color.yellow;

    private EnemyAIContext context;
    private EnemyStateMachine stateMachine;

    public override bool IsBusy => context != null && context.IsBusy;

    private void Awake()
    {
        ResolveLocalReferences();
        CreateStateMachine();
        ResolveSceneReferences();
    }

    /// <summary>
    /// Attempts to perform one turn-based AI action through the state machine.
    /// </summary>
    public override bool PerformAI()
    {
        // Scene references can be created after this enemy awakens (e.g. by the dungeon generator).
        ResolveSceneReferences();
        return stateMachine.PerformAction();
    }

    private void ResolveLocalReferences()
    {
        // Explicit Unity null checks: '??=' would skip assigning "missing" serialized references.
        if (enemy == null) enemy = GetComponent<Enemy>();
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Builds the runtime state machine. Existing prefabs keep this script attached, so the
    /// mover is added from code when missing.
    /// </summary>
    private void CreateStateMachine()
    {
        if (!TryGetComponent(out GridMover mover))
        {
            mover = gameObject.AddComponent<GridMover>();
        }

        stateMachine = GetComponent<EnemyStateMachine>();

        var config = new EnemyAIConfig(
            moveSpeed,
            spotRange,
            retreatHealth,
            attackDamage,
            attackRange,
            critChance,
            critMultiplier);

        var audio = new EnemyAIAudio(moveSound, attackSound, critSound);

        context = new EnemyAIContext(transform, mover, enemy, enemyHealth, animator, audioSource, config, audio);

        stateMachine.Initialize(
            context,
            new PatrolState(),
            new RetreatState(),
            new AttackState(),
            new ChaseState());
    }

    private void ResolveSceneReferences()
    {
        GameObjectLocator locator = GameObjectLocator.Instance;
        if (locator == null)
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
            return;
        }

        Player player = locator.Player;
        PlayerHealth playerHealth = locator.GetComponentByTag<PlayerHealth>("Player");
        Tilemap tilemap = locator.Tilemap;
        ObstacleTilemap obstacleTilemap = locator.ObstacleTilemap;

        context.SetSceneReferences(player, playerHealth, tilemap, obstacleTilemap);
    }

    #region Testing
    /// <summary>
    /// Draws gizmos in the editor to visualize the spot range.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = spotRangeColor;
        Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);

        for (int dx = -spotRange; dx <= spotRange; dx++)
        {
            for (int dy = -spotRange; dy <= spotRange; dy++)
            {
                if (Mathf.Abs(dx) + Mathf.Abs(dy) <= spotRange)
                {
                    Vector2 worldPos = GridUtils.GridToWorld(enemyGrid + new Vector2Int(dx, dy));
                    Gizmos.DrawWireCube(worldPos, new Vector3(gridSize.x, gridSize.y, 0.1f));
                }
            }
        }
    }
    #endregion Testing
}
