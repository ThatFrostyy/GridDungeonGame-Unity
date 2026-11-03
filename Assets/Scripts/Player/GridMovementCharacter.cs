using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the player's mouse input: moving to adjacent tiles and attacking enemies.
/// </summary>
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

    private GridMover mover;
    private ObstacleTilemap obstacleTilemap;
    private TileSelection tileSelection;
    private int enemyLayerMask;

    private void Awake()
    {
        mover = GetComponent<GridMover>();
        if (mover == null)
        {
            mover = gameObject.AddComponent<GridMover>();
        }

        enemyLayerMask = LayerMask.GetMask("Enemy");
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        if (GameObjectLocator.Instance == null)
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
            return;
        }

        obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemap;
        tileSelection = GameObjectLocator.Instance.TileSelection;
    }

    private void Update()
    {
        if (mover.IsMoving || !Input.GetMouseButtonDown(0) || tileSelection == null)
        {
            return;
        }

        if (tpMode)
        {
            TeleportToTile();
            return;
        }

        HandleClick();
    }

    /// <summary>
    /// Handles a click on the map: attack an enemy on the tile, or step onto an adjacent free tile.
    /// </summary>
    private void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!CanPlayerAct())
        {
            Debug.Log("Not your turn or no action points.");
            return;
        }

        Vector2 targetPos = tileSelection.GetHighlightedTileWorldPosition();

        EnemyHealth enemy = GetEnemyAtPosition(targetPos);
        if (enemy != null)
        {
            mover.FaceToward(enemy.transform.position);
            player.Attack(enemy);
            return;
        }

        Vector2Int clickedTile = GridUtils.WorldToGrid(targetPos);
        Vector2Int playerTile = GridUtils.WorldToGrid(transform.position);

        if (!GridUtils.IsAdjacent(clickedTile, playerTile))
        {
            Debug.Log("You can only move to adjacent tiles.");
            return;
        }

        if (obstacleTilemap.IsTileObstacle(clickedTile))
        {
            return;
        }

        MoveToTile(clickedTile);
    }

    private bool CanPlayerAct()
    {
        return TurnManager.Instance != null && TurnManager.Instance.PlayerTurn && player.CurrentActionPoints > 0;
    }

    private EnemyHealth GetEnemyAtPosition(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position, enemyLayerMask);
        return hit != null ? hit.GetComponent<EnemyHealth>() : null;
    }

    private void MoveToTile(Vector2Int tile)
    {
        if (!player.UseActionPoint())
        {
            Debug.Log("Not enough Action Points!");
            return;
        }

        SetWalking(true);
        AudioUtils.PlayOneShot(audioSource, moveSound);
        mover.MoveTo(GridUtils.GridToWorld(tile), moveSpeed, () => SetWalking(false));
    }

    private void SetWalking(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool(AnimatorParams.IsWalking, isWalking);
        }
    }

    /// <summary>
    /// Cheat mode: instantly moves to the highlighted tile if it is not blocked.
    /// </summary>
    private void TeleportToTile()
    {
        Vector2 target = tileSelection.GetHighlightedTileWorldPosition();

        if (!obstacleTilemap.IsTileObstacle(GridUtils.WorldToGrid(target)))
        {
            mover.Teleport(target);
        }
    }
}
