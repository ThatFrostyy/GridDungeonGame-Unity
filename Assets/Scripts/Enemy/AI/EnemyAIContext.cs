using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Shared data and reusable actions for enemy AI states.
/// </summary>
public sealed class EnemyAIContext
{
    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

    private readonly MonoBehaviour coroutineHost;
    private readonly Transform self;
    private readonly Animator animator;
    private readonly AudioSource audioSource;
    private readonly AudioClip moveSound;
    private readonly AudioClip attackSound;
    private readonly AudioClip critSound;
    private readonly float moveSpeed;
    private readonly float retreatHealth;
    private readonly Vector2 gridSize;
    private readonly int attackDamage;
    private readonly int attackRange;
    private readonly float critChance;
    private readonly float critMultiplier;

    private Player player;
    private PlayerHealth playerHealth;
    private Tilemap tilemap;
    private ObstacleTilemap obstacleTilemap;
    private bool isMoving;

    public EnemyAIContext(
        MonoBehaviour coroutineHost,
        Transform self,
        Player player,
        PlayerHealth playerHealth,
        Enemy enemy,
        EnemyHealth enemyHealth,
        Tilemap tilemap,
        ObstacleTilemap obstacleTilemap,
        Animator animator,
        AudioSource audioSource,
        AudioClip moveSound,
        AudioClip attackSound,
        AudioClip critSound,
        float moveSpeed,
        int spotRange,
        float retreatHealth,
        Vector2 gridSize,
        int attackDamage,
        int attackRange,
        float critChance,
        float critMultiplier)
    {
        this.coroutineHost = coroutineHost;
        this.self = self;
        this.player = player;
        this.playerHealth = playerHealth;
        Enemy = enemy;
        EnemyHealth = enemyHealth;
        this.tilemap = tilemap;
        this.obstacleTilemap = obstacleTilemap;
        this.animator = animator;
        this.audioSource = audioSource;
        this.moveSound = moveSound;
        this.attackSound = attackSound;
        this.critSound = critSound;
        this.moveSpeed = moveSpeed;
        SpotRange = spotRange;
        this.retreatHealth = retreatHealth;
        this.gridSize = gridSize;
        this.attackDamage = attackDamage;
        this.attackRange = attackRange;
        this.critChance = critChance;
        this.critMultiplier = critMultiplier;
    }

    public Enemy Enemy { get; }
    public EnemyHealth EnemyHealth { get; }
    public int SpotRange { get; }
    public bool IsBusy => isMoving;

    public bool CanAct => !isMoving && player != null && Enemy != null && EnemyHealth != null && Enemy.IsAlive;
    public bool IsPlayerOutsideSpotRange => CanAct && DistanceToPlayer() > SpotRange;
    public bool ShouldRetreat => CanAct && EnemyHealth.HealthPercentage < retreatHealth;
    public bool IsPlayerInAttackRange => CanAct && DistanceToPlayer() <= attackRange;

    /// <summary>
    /// Updates references that can become available after Awake execution order.
    /// </summary>
    public void UpdateSceneReferences(Player player, PlayerHealth playerHealth, Tilemap tilemap, ObstacleTilemap obstacleTilemap)
    {
        this.player = player;
        this.playerHealth = playerHealth;
        this.tilemap = tilemap;
        this.obstacleTilemap = obstacleTilemap;
    }

    /// <summary>
    /// Tries random adjacent movement while the player is outside spotting range.
    /// </summary>
    public bool TryPatrol()
    {
        if (!CanAct)
        {
            return false;
        }

        Vector2Int enemyGrid = CurrentGridPosition();

        for (int attempt = 0; attempt < 3; attempt++)
        {
            Vector2Int direction = CardinalDirections[Random.Range(0, CardinalDirections.Length)];
            if (TryMoveToGridPosition(enemyGrid + direction))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Moves to the adjacent tile that maximizes distance from the player.
    /// </summary>
    public bool TryRetreat()
    {
        if (!CanAct)
        {
            return false;
        }

        Vector2Int enemyGrid = CurrentGridPosition();
        Vector2Int playerGrid = PlayerGridPosition();
        Vector2Int bestTile = enemyGrid;
        int bestDistance = ManhattanDistance(enemyGrid, playerGrid);

        foreach (Vector2Int direction in CardinalDirections)
        {
            Vector2Int candidate = enemyGrid + direction;
            if (candidate == playerGrid || !IsWalkable(candidate))
            {
                continue;
            }

            int distance = ManhattanDistance(candidate, playerGrid);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestTile = candidate;
            }
        }

        return bestTile != enemyGrid && TryMoveToGridPosition(bestTile);
    }

    /// <summary>
    /// Moves one tile along an A* path toward the player.
    /// </summary>
    public bool TryChasePlayer()
    {
        if (!CanAct)
        {
            return false;
        }

        Vector2 startPosition = GridUtils.GridToWorld(CurrentGridPosition());
        Vector2 targetPosition = GridUtils.GridToWorld(PlayerGridPosition());
        List<Vector2> path = AStar.FindPath(startPosition, targetPosition, gridSize, IsObstacle);

        if (path == null || path.Count <= 1)
        {
            return false;
        }

        Vector2 nextPosition = path[1];
        if (GridUtils.WorldToGrid(nextPosition) == PlayerGridPosition())
        {
            return false;
        }

        FacePosition(nextPosition);
        return StartMove(nextPosition);
    }

    /// <summary>
    /// Attacks the player and applies critical-hit modifiers.
    /// </summary>
    public bool TryAttackPlayer()
    {
        if (!CanAct || playerHealth == null || !IsPlayerInAttackRange)
        {
            return false;
        }

        FacePosition(player.transform.position);

        int finalDamage = attackDamage;
        bool isCriticalHit = Random.value < critChance;

        if (isCriticalHit)
        {
            finalDamage = Mathf.RoundToInt(attackDamage * critMultiplier);
            PlayOneShot(critSound, randomizePitch: false);
        }
        else
        {
            PlayOneShot(attackSound, randomizePitch: true);
        }

        playerHealth.TakeDamage(finalDamage);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        return true;
    }

    private Vector2Int CurrentGridPosition()
    {
        return GridUtils.WorldToGrid(self.position);
    }

    private Vector2Int PlayerGridPosition()
    {
        return GridUtils.WorldToGrid(player.transform.position);
    }

    private int DistanceToPlayer()
    {
        return ManhattanDistance(CurrentGridPosition(), PlayerGridPosition());
    }

    private static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private bool TryMoveToGridPosition(Vector2Int gridPosition)
    {
        if (!IsWalkable(gridPosition))
        {
            return false;
        }

        return StartMove(GridUtils.GridToWorld(gridPosition));
    }

    private bool IsWalkable(Vector2Int gridPosition)
    {
        if (tilemap != null && !tilemap.HasTile(new Vector3Int(gridPosition.x, gridPosition.y, 0)))
        {
            return false;
        }

        return !IsObstacle(GridUtils.GridToWorld(gridPosition));
    }

    private bool IsObstacle(Vector2 position)
    {
        return obstacleTilemap != null && obstacleTilemap.IsTileObstacle(position);
    }

    private bool StartMove(Vector2 targetPosition)
    {
        if (coroutineHost == null || isMoving)
        {
            return false;
        }

        coroutineHost.StartCoroutine(MoveAlongPath(targetPosition));
        return true;
    }

    private IEnumerator MoveAlongPath(Vector2 targetPosition)
    {
        isMoving = true;
        SetWalkingAnimation(true);
        PlayOneShot(moveSound, randomizePitch: false);

        while ((Vector2)self.position != targetPosition)
        {
            float step = moveSpeed * Time.deltaTime;
            self.position = Vector3.MoveTowards(self.position, targetPosition, step);
            yield return new WaitForFixedUpdate();
        }

        SetWalkingAnimation(false);
        isMoving = false;
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

    private void FacePosition(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)self.position).normalized;
        if (Mathf.Abs(direction.x) <= 0.01f)
        {
            return;
        }

        Vector3 scale = self.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        self.localScale = scale;
    }

    private void PlayOneShot(AudioClip clip, bool randomizePitch)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
        }

        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1f;
    }
}
