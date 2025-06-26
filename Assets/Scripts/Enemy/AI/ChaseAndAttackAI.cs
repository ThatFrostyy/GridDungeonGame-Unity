using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Enemy AI that chases, attacks, patrols, or retreats based on player proximity and health.
/// </summary>
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
    private Color spotRangeColor = Color.yellow;

    private bool isMoving = false;

    private Player player;
    private PlayerHealth playerHealth;
    private Tilemap tilemap;
    private ObstacleTilemap obstacleTilemap;


    /// <summary>
    /// Initializes references using the GameObjectLocator
    /// </summary>
    private void Awake()
    {
        InitializeReferences();
    }

    /// <summary>
    /// Gets necessary references from the GameObjectLocator
    /// </summary>
    private void InitializeReferences()
    {
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
    /// Attempts to perform a single AI action: patrol, retreat, chase, or attack.
    /// </summary>
    /// <returns>True if an action was performed, otherwise false.</returns>
    public override bool PerformAI()
    {
        if (isMoving || player == null || enemy == null || enemyHealth == null)
            return false;

        Vector2Int e = GridUtils.WorldToGrid(transform.position);
        Vector2Int p = GridUtils.WorldToGrid(player.transform.position);
        int dist = Mathf.Abs(e.x - p.x) + Mathf.Abs(e.y - p.y);

        if (dist > spotRange)
            return Patrol();

        if (enemyHealth.HealthPercentage < retreatHealth)
            return Retreat();

        if (IsWithinAttackRange(e, p))
        {
            AttackPlayer();
            return true;
        }

        return TryMoveTowardsPlayer(p);
    }


    /// <summary>
    /// Patrols randomly to a nearby tile if possible.
    /// </summary>
    /// <returns>True if patrol movement started, otherwise false.</returns>
    private bool Patrol()
    {
        Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };


        for (int attempt = 0; attempt < 3; attempt++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int targetGrid = enemyGrid + dir;

            if (tilemap != null && !tilemap.HasTile(new Vector3Int(targetGrid.x, targetGrid.y, 0)))
                continue;

            if (obstacleTilemap != null && obstacleTilemap.IsTileObstacle(GridUtils.GridToWorld(targetGrid)))
                continue;

            Vector2 targetWorld = GridUtils.GridToWorld(targetGrid);
            StartCoroutine(MoveAlongPath(targetWorld));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retreat to the farthest adjacent tile from the player.
    /// </summary>
    /// <returns>True if retreat movement started, otherwise false.</returns>
    private bool Retreat()
    {
        Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);
        Vector2Int playerGrid = GridUtils.WorldToGrid(player.transform.position);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        Vector2Int bestTile = enemyGrid;
        int maxDist = Mathf.Abs(enemyGrid.x - playerGrid.x) + Mathf.Abs(enemyGrid.y - playerGrid.y);

        foreach (var dir in directions)
        {
            Vector2Int candidate = enemyGrid + dir;
            if (obstacleTilemap != null && obstacleTilemap.IsTileObstacle(GridUtils.GridToWorld(candidate)))
                continue;

            if (candidate == playerGrid)
                continue;

            int dist = Mathf.Abs(candidate.x - playerGrid.x) + Mathf.Abs(candidate.y - playerGrid.y);
            if (dist > maxDist)
            {
                maxDist = dist;
                bestTile = candidate;
            }
        }

        if (bestTile != enemyGrid)
        {
            Vector2 nextPos = GridUtils.GridToWorld(bestTile);
            StartCoroutine(MoveAlongPath(nextPos));
            return true; 
        }

        return false; 
    }

    /// <summary>
    /// Attempts to move towards the player using A* pathfinding.
    /// </summary>
    /// <param name="playerGrid">Grid position of the player.</param>
    /// <returns>True if movement started, otherwise false.</returns>
    private bool TryMoveTowardsPlayer(Vector2Int playerGrid)
    {
        Vector2 startPos = GridUtils.GridToWorld(GridUtils.WorldToGrid(transform.position));
        Vector2 targetPos = GridUtils.GridToWorld(playerGrid);

        List<Vector2> path = AStar.FindPath(startPos, targetPos, gridSize, obstacleTilemap.IsTileObstacle);

        if (path != null && path.Count > 1)
        {
            Vector2Int nextGrid = GridUtils.WorldToGrid(path[1]);
            Vector2Int playerGridPos = GridUtils.WorldToGrid(player.transform.position);

            if (nextGrid == playerGridPos)
                return false;

            Vector2 nextPos = path[1];
            Vector2 direction = (nextPos - (Vector2)transform.position).normalized;
            SetFacing(direction.x);

            StartCoroutine(MoveAlongPath(nextPos));
            return true;
        }
        return false;
    }

    #region Helper Methods
    /// <summary>
    /// Checks if two grid positions are adjacent (Manhattan distance 1).
    /// </summary>
    /// <param name="a">First grid position.</param>
    /// <param name="b">Second grid position.</param>
    /// <returns>True if adjacent, otherwise false.</returns>
    private bool IsWithinAttackRange(Vector2Int a, Vector2Int b)
    {
        int dist = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        return dist <= attackRange;
    }


    /// <summary>
    /// Moves the enemy along a path to the specified position.
    /// </summary>
    /// <param name="nextPos">World position to move to.</param>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator MoveAlongPath(Vector2 nextPos)
    {
        isMoving = true;
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }

        if (moveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(moveSound);
        }

        while ((Vector2)transform.position != nextPos)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, nextPos, step);
            yield return new WaitForFixedUpdate();
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
        isMoving = false;
    }

    /// <summary>
    /// Attacks the player, applying damage and playing effects.
    /// </summary>
    private void AttackPlayer()
    {
        if (player == null || playerHealth == null) return;

        int finalDamage = attackDamage;
        bool isCrit = Random.value < critChance;

        if (isCrit)
        {
            finalDamage = Mathf.RoundToInt(attackDamage * critMultiplier);
            Debug.Log($"CRITICAL HIT! Dealt {finalDamage} damage.");

            if (audioSource != null && critSound != null)
            {
                audioSource.PlayOneShot(critSound);
            }
        }
        else
        {
            if (audioSource != null && attackSound != null)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(attackSound);
                audioSource.pitch = 1f;
            }
        }

        playerHealth.TakeDamage(finalDamage);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// Sets the facing direction of the enemy sprite based on movement.
    /// </summary>
    /// <param name="xDirection">X direction of movement.</param>
    private void SetFacing(float xDirection)
    {
        if (Mathf.Abs(xDirection) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = xDirection < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
    #endregion Helper Methods

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