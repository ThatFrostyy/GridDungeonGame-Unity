using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Turn-based enemy AI facade that delegates behavior selection to a state machine.
/// </summary>
[RequireComponent(typeof(EnemyStateMachine))]
public class ChaseAndAttackAI : EnemyAI
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int spotRange = 3;
    [Tooltip("Health percentage below which the enemy will retreat.")]
    [Range(0f, 1f)] [SerializeField] private float retreatHealth = 0.3f;
    [SerializeField] private Vector2 gridSize = new(1f, 1f);

    [Header("Combat Settings")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private int attackRange = 1; 
    [Range(0f, 1f)][SerializeField] private float critChance = 0.2f;
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
    private Player player;
    private PlayerHealth playerHealth;
    private Tilemap tilemap;
    private ObstacleTilemap obstacleTilemap;

    /// <summary>
    /// Initializes references and behavior states.
    /// </summary>
    private void Awake()
    {
        InitializeReferences();
        InitializeStateMachine();
    }

    /// <summary>
    /// Gets scene references from the GameObjectLocator.
    /// </summary>
    private void InitializeReferences()
    {
        enemy ??= GetComponent<Enemy>();
        enemyHealth ??= GetComponent<EnemyHealth>();
        audioSource ??= GetComponent<AudioSource>();
        animator ??= GetComponentInChildren<Animator>();

        if (GameObjectLocator.Instance != null)
        {
            player = GameObjectLocator.Instance.Player;
            playerHealth = GameObjectLocator.Instance.GetComponentByTag<PlayerHealth>("Player");
            obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemap;
            tilemap = GameObjectLocator.Instance.Tilemap;
        }
        else
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
        }
    }

    /// <summary>
    /// Creates the runtime state machine. Existing prefabs keep this script attached,
    /// so the FSM is created from code when the component is missing.
    /// </summary>
    private void InitializeStateMachine()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<EnemyStateMachine>();
        }

        context = new EnemyAIContext(
            this,
            transform,
            player,
            playerHealth,
            enemy,
            enemyHealth,
            tilemap,
            obstacleTilemap,
            animator,
            audioSource,
            moveSound,
            attackSound,
            critSound,
            moveSpeed,
            spotRange,
            retreatHealth,
            gridSize,
            attackDamage,
            attackRange,
            critChance,
            critMultiplier);

        stateMachine.Initialize(
            context,
            new PatrolState(),
            new RetreatState(),
            new AttackState(),
            new ChaseState());
    }

    /// <summary>
    /// Refreshes scene references that can be created after this component awakens.
    /// </summary>
    private void RefreshContextReferences()
    {
        InitializeReferences();
        context?.UpdateSceneReferences(player, playerHealth, tilemap, obstacleTilemap);
    }

    /// <summary>
    /// Attempts to perform one turn-based AI action through the state machine.
    /// </summary>
    public override bool PerformAI()
    {
        RefreshContextReferences();
        return stateMachine != null && stateMachine.PerformAction();
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
                    Vector2Int tile = enemyGrid + new Vector2Int(dx, dy);
                    Vector2 worldPos = GridUtils.GridToWorld(tile);
                    Gizmos.DrawWireCube(worldPos, new Vector3(gridSize.x, gridSize.y, 0.1f));
                }
            }
        }
    }
    #endregion Testing
}
