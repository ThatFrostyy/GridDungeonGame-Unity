using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Shared data and low-level actions that enemy AI states build their behavior on.
/// Decision making lives in the states; this class only answers questions and performs primitives.
/// </summary>
public sealed class EnemyAIContext
{
    private readonly Transform self;
    private readonly GridMover mover;
    private readonly Animator animator;
    private readonly AudioSource audioSource;

    private Player player;
    private PlayerHealth playerHealth;
    private Tilemap tilemap;
    private ObstacleTilemap obstacleTilemap;
    private int enemyLayerMask = -1;

    public EnemyAIContext(
        Transform self,
        GridMover mover,
        Enemy enemy,
        EnemyHealth enemyHealth,
        Animator animator,
        AudioSource audioSource,
        EnemyAIConfig config,
        EnemyAIAudio audio)
    {
        this.self = self;
        this.mover = mover;
        this.animator = animator;
        this.audioSource = audioSource;
        Enemy = enemy;
        EnemyHealth = enemyHealth;
        Config = config;
        Audio = audio;
    }

    public Enemy Enemy { get; }
    public EnemyHealth EnemyHealth { get; }
    public EnemyAIConfig Config { get; }
    public EnemyAIAudio Audio { get; }
    public PlayerHealth PlayerHealth => playerHealth;
    public Vector2 PlayerPosition => player.transform.position;

    public bool IsBusy => mover.IsMoving;
    public bool CanAct => !IsBusy && player != null && Enemy != null && EnemyHealth != null && Enemy.IsAlive;

    public bool IsPlayerOutsideSpotRange => CanAct && DistanceToPlayer() > Config.SpotRange;
    public bool IsPlayerInAttackRange => CanAct && DistanceToPlayer() <= Config.AttackRange;
    public bool ShouldRetreat => CanAct && EnemyHealth.HealthPercentage < Config.RetreatHealth;

    public Vector2Int GridPosition => GridUtils.WorldToGrid(self.position);
    public Vector2Int PlayerGridPosition => GridUtils.WorldToGrid(player.transform.position);

    public int DistanceToPlayer()
    {
        return GridUtils.ManhattanDistance(GridPosition, PlayerGridPosition);
    }

    /// <summary>
    /// Updates scene references that may only become available after this enemy awakens.
    /// </summary>
    public void SetSceneReferences(Player player, PlayerHealth playerHealth, Tilemap tilemap, ObstacleTilemap obstacleTilemap)
    {
        this.player = player;
        this.playerHealth = playerHealth;
        this.tilemap = tilemap;
        this.obstacleTilemap = obstacleTilemap;
    }

    /// <summary>
    /// True if the tile is on the map, free of obstacles and not occupied by the player or another enemy.
    /// </summary>
    public bool IsWalkable(Vector2Int gridPosition)
    {
        if (tilemap != null && !tilemap.HasTile(new Vector3Int(gridPosition.x, gridPosition.y, 0)))
        {
            return false;
        }

        if (gridPosition == PlayerGridPosition)
        {
            return false;
        }

        Vector2 worldPosition = GridUtils.GridToWorld(gridPosition);
        return !IsObstacle(worldPosition) && !IsOccupiedByOtherEnemy(worldPosition);
    }

    public bool IsObstacle(Vector2 worldPosition)
    {
        return obstacleTilemap != null && obstacleTilemap.IsTileObstacle(worldPosition);
    }

    /// <summary>
    /// Starts walking to a tile if it is walkable.
    /// </summary>
    public bool TryMoveTo(Vector2Int gridPosition)
    {
        return IsWalkable(gridPosition) && StartMove(GridUtils.GridToWorld(gridPosition));
    }

    /// <summary>
    /// Starts moving one tile along an A* path toward the player.
    /// </summary>
    public bool TryStepTowardPlayer()
    {
        List<Vector2> path = AStar.FindPath(
            GridUtils.GridToWorld(GridPosition),
            GridUtils.GridToWorld(PlayerGridPosition),
            IsObstacle);

        if (path == null || path.Count <= 1)
        {
            return false;
        }

        return TryMoveTo(GridUtils.WorldToGrid(path[1]));
    }

    public void FacePosition(Vector2 worldPosition)
    {
        mover.FaceToward(worldPosition);
    }

    public void PlaySound(AudioClip clip, bool randomizePitch = false)
    {
        AudioUtils.PlayOneShot(audioSource, clip, randomizePitch);
    }

    public void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(AnimatorParams.Attack);
        }
    }

    private bool StartMove(Vector2 targetPosition)
    {
        if (!mover.MoveTo(targetPosition, Config.MoveSpeed, OnMoveFinished))
        {
            return false;
        }

        if (animator != null)
        {
            animator.SetBool(AnimatorParams.IsWalking, true);
        }

        PlaySound(Audio.Move);
        return true;
    }

    private void OnMoveFinished()
    {
        if (animator != null)
        {
            animator.SetBool(AnimatorParams.IsWalking, false);
        }
    }

    private bool IsOccupiedByOtherEnemy(Vector2 worldPosition)
    {
        if (enemyLayerMask < 0)
        {
            enemyLayerMask = LayerMask.GetMask("Enemy");
        }

        Collider2D hit = Physics2D.OverlapPoint(worldPosition, enemyLayerMask);
        return hit != null && !hit.transform.IsChildOf(self);
    }
}
