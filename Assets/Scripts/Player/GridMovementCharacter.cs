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

    private void Awake()
    {
        GameObject obstacleTilemapObject = GameObject.FindGameObjectWithTag("ObstacleTilemap");
        GameObject tileSelectionObject = GameObject.FindGameObjectWithTag("TileSelection");
        if (obstacleTilemapObject != null)
        {
            obstacleTilemap = obstacleTilemapObject.GetComponent<ObstacleTilemap>();
        }
        else
        {
            Debug.LogWarning("ObstacleTilemap not found. Ensure a Tilemap with the 'ObstacleTilemap' tag exists in the scene.");
        }
        if (tileSelectionObject != null)
        {
            tileSelection = tileSelectionObject.GetComponent<TileSelection>();
        }
        else
        {
            Debug.LogWarning("TileSelection not found. Ensure a GameObject with the 'TileSelection' tag exists in the scene.");
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

    private void HandleMovementInputs()
    {
        if (!isMoving && Input.GetMouseButtonDown(0) && !tpMode)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (TurnManager.Instance.PlayerTurn && player.CurrentActionPoints > 0)
            {
                targetPosition = tileSelection.GetHighlightedTileWorldPosition();
                Vector2Int clickedTile = GridUtils.WorldToGrid(targetPosition);
                Vector2Int playerGrid = GridUtils.WorldToGrid(transform.position);

                // Check for enemy on clicked tile
                Collider2D hit = Physics2D.OverlapPoint(targetPosition, LayerMask.GetMask("Enemy"));
                EnemyHealth enemy = hit ? hit.GetComponent<EnemyHealth>() : null;

                int dx = Mathf.Abs(clickedTile.x - playerGrid.x);
                int dy = Mathf.Abs(clickedTile.y - playerGrid.y);

                if (enemy != null)
                {
                    if ((dx + dy) == 1)
                    {
                        if (player.UseActionPoint())
                        {
                            FacePosition(enemy.transform.position);
                            player.Attack(enemy);
                        }
                    }
                    else
                    {
                        Debug.Log("Enemy must be adjacent to attack.");
                    }
                    return;
                }

                if (!obstacleTilemap.IsTileObstacle(clickedTile))
                {
                    if ((dx + dy) != 1)
                    {
                        Debug.Log("You can only move to adjacent tiles.");
                        return;
                    }

                    if (player.UseActionPoint())
                    {
                        List<Vector2> path = new() { GridUtils.GridToWorld(clickedTile) };
                        StartCoroutine(MoveAlongPath(path));
                    }
                    else
                    {
                        Debug.Log("Not enough Action Points!");
                    }
                }
            }
            else
            {
                Debug.Log("Not your turn or no action points.");
            }
        }

        if (isMoving)
        {
            MoveTowardsTarget();
        }

        if (Input.GetMouseButtonDown(0) && tpMode)
        {
            TeleportToTile();
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