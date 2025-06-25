using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MovementDirection
{
    Up,
    Down,
    Left,
    Right
}

public class GridMovementCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sprite Settings")]
    [SerializeField] private Animator animator;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;

    [Header("Cheat Settings")]
    [SerializeField] private bool tpMode = false;

    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool facingLeft = false;

    private ObstacleTilemap obstacleTilemap;
    private TileSelection tileSelection;

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
            obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemap;
            tileSelection = GameObjectLocator.Instance.TileSelection;
        }
        else
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
        }
    }

    private void Update()
    {
        HandleMovementInputs();

        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }
    }

    /// <summary>
    /// Handles all player movement inputs
    /// </summary>
    private void HandleMovementInputs()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
            return;
        }

        if (!Input.GetMouseButtonDown(0))
            return;

        if (tpMode)
        {
            TeleportToTile();
            return;
        }

        HandleNormalMovement();
    }

    /// <summary>
    /// Handles normal player movement (not teleport mode)
    /// </summary>
    private void HandleNormalMovement()
    {
        // Ignore clicks on UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Check if it's player's turn and has action points
        if (!CanPlayerAct())
        {
            Debug.Log("Not your turn or no action points.");
            return;
        }

        Vector2 targetPos = tileSelection.GetHighlightedTileWorldPosition();
        Vector2Int clickedTile = GridUtils.WorldToGrid(targetPos);
        Vector2Int playerGrid = GridUtils.WorldToGrid(transform.position);

        // Check if there's an enemy on the clicked tile
        EnemyHealth enemy = GetEnemyAtPosition(targetPos);
        if (enemy != null)
        {
            HandleAttackAction(enemy);
            return;
        }

        // Check if click is on an adjacent tile
        if (!IsAdjacentTile(clickedTile, playerGrid))
        {
            Debug.Log("You can only move to adjacent tiles.");
            return;
        }

        // Check if tile is passable
        if (obstacleTilemap.IsTileObstacle(clickedTile))
            return;

        // Execute movement
        HandleMovementAction(clickedTile);
    }

    /// <summary>
    /// Checks if the player can act (turn and action points)
    /// </summary>
    private bool CanPlayerAct()
    {
        return TurnManager.Instance.PlayerTurn && player.CurrentActionPoints > 0;
    }

    /// <summary>
    /// Checks if a tile is adjacent to the player
    /// </summary>
    private bool IsAdjacentTile(Vector2Int targetTile, Vector2Int playerTile)
    {
        int dx = Mathf.Abs(targetTile.x - playerTile.x);
        int dy = Mathf.Abs(targetTile.y - playerTile.y);
        return (dx + dy) == 1;
    }

    /// <summary>
    /// Gets the enemy at a specific position
    /// </summary>
    private EnemyHealth GetEnemyAtPosition(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position, LayerMask.GetMask("Enemy"));
        return hit ? hit.GetComponent<EnemyHealth>() : null;
    }

    /// <summary>
    /// Handles the attack action against an enemy
    /// </summary>
    private void HandleAttackAction(EnemyHealth enemy)
    {
         FacePosition(enemy.transform.position);
         player.Attack(enemy);
    }

    /// <summary>
    /// Handles the movement action to a tile
    /// </summary>
    private void HandleMovementAction(Vector2Int targetTile)
    {
        if (player.UseActionPoint())
        {
            targetPosition = GridUtils.GridToWorld(targetTile);
            List<Vector2> path = new() { targetPosition };
            StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            Debug.Log("Not enough Action Points!");
        }
    }
    private IEnumerator MoveAlongPath(List<Vector2> path)
    {
        isMoving = true;
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < path.Count)
        {
            targetPosition = path[currentWaypointIndex];

            if (moveSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(moveSound);
            }

            while ((Vector2)transform.position != targetPosition)
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                yield return new WaitForFixedUpdate();
            }

            currentWaypointIndex++;
        }

        isMoving = false;
    }

    private void MoveTowardsTarget()
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                SetFacing(false);
            }
            else
            {
                SetFacing(true);
            }
        }
        else
        {
            if (direction.y > 0)
            {

            }
            else
            {

            }
        }
    }

    private void SetFacing(bool left)
    {
        if (facingLeft == left)
        {
            return;
        }

        facingLeft = left;
        Vector3 scale = transform.localScale;
        scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void FacePosition(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            SetFacing(direction.x < 0);
        }
    }

    private void TeleportToTile()
    {
        Vector2 target = tileSelection.GetHighlightedTileWorldPosition();
        Vector2Int gridPos = GridUtils.WorldToGrid(target);

        if (!obstacleTilemap.IsTileObstacle(gridPos))
        {
            transform.position = target;
            isMoving = false;
        }
    }
}