using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChaseAndAttackAI : EnemyAI
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int spotRange = 3;
    [SerializeField] private float retreatHealth = 0.3f; // 30% health to retreat
    [SerializeField] private Vector2 gridSize = new(1f, 1f);

    [Header("Sprite Settings")]
    [SerializeField] private Animator animator;

    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private Enemy enemyComponent;
    [SerializeField] private EnemyHealth enemyHealth;

    private Player playerComponent;

    private bool isMoving = false;

    private ObstacleTilemap obstacleTilemap;
    private Tilemap tilemap;

    private void Awake()
    {
        GameObject obstacleTilemapObject = GameObject.FindGameObjectWithTag("ObstacleTilemap");
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        GameObject tilemap = GameObject.FindGameObjectWithTag("Tilemap");
        if (tilemap != null)
        {
            this.tilemap = tilemap.GetComponent<Tilemap>();
        }
        else
        {
            Debug.LogWarning("Tilemap not found. Ensure a Tilemap with the 'Tilemap' tag exists in the scene.");
        }
        if (obstacleTilemapObject != null)
        {
            obstacleTilemap = obstacleTilemapObject.GetComponent<ObstacleTilemap>();
        }
        else
        {
            Debug.LogWarning("ObstacleTilemap not found. Ensure a Tilemap with the 'ObstacleTilemap' tag exists in the scene.");
        }
        if (playerObject != null)
        {
            playerComponent = playerObject.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player object not found. Ensure the player has the 'Player' tag.");
        }
    }

    public override void PerformAI()
    {
        if (isMoving || playerComponent == null || enemyComponent == null || enemyHealth == null)
        {
            return;
        }

        Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);
        Vector2Int playerGrid = GridUtils.WorldToGrid(playerComponent.transform.position);
        int gridDistance = Mathf.Abs(enemyGrid.x - playerGrid.x) + Mathf.Abs(enemyGrid.y - playerGrid.y);

        if (gridDistance > spotRange)
        {
            Patrol();
            return;
        }

        float healthPercent = (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth;
        if (healthPercent < retreatHealth)
        {
            StartCoroutine(RetreatRoutine());
            return;
        }

        // Otherwise, act smart
        StartCoroutine(SmartTurnRoutine());
    }

    private IEnumerator SmartTurnRoutine()
    {
        while (enemyComponent.CurrentActionPoints > 0)
        {
            Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);
            Vector2Int playerGrid = GridUtils.WorldToGrid(playerComponent.transform.position);

            if (IsAdjacent(enemyGrid, playerGrid))
            {
                AttackPlayer();
                enemyComponent.UseActionPoint();
                yield return new WaitForSeconds(TurnManager.Instance.EnemyActionDelay);
            }
            else
            {
                bool moved = TryMoveTowardsPlayer(playerGrid);
                if (moved)
                {
                    enemyComponent.UseActionPoint();
                    yield return new WaitUntil(() => !isMoving);
                    yield return new WaitForSeconds(TurnManager.Instance.EnemyActionDelay);
                }
                else
                {
                    break;
                }
            }
        }
    }

    private IEnumerator RetreatRoutine()
    {
        while (enemyComponent.CurrentActionPoints > 0)
        {
            Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);
            Vector2Int playerGrid = GridUtils.WorldToGrid(playerComponent.transform.position);

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            Vector2Int bestTile = enemyGrid;
            int maxDist = Mathf.Abs(enemyGrid.x - playerGrid.x) + Mathf.Abs(enemyGrid.y - playerGrid.y);

            foreach (var dir in directions)
            {
                Vector2Int candidate = enemyGrid + dir;
                if (obstacleTilemap != null && obstacleTilemap.IsTileObstacle(GridUtils.GridToWorld(candidate)))
                {
                    continue;
                }

                if (candidate == playerGrid)
                {
                    continue;
                }

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
                enemyComponent.UseActionPoint();
                yield return new WaitUntil(() => !isMoving);
                yield return new WaitForSeconds(TurnManager.Instance.EnemyActionDelay);
            }
            else
            {
                // No retreat possible, just wait
                yield return new WaitForSeconds(TurnManager.Instance.EnemyActionDelay); break;
            }
        }
    }

    private void Patrol()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        Vector2Int enemyGrid = GridUtils.WorldToGrid(transform.position);

        for (int attempt = 0; attempt < 1; attempt++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int targetGrid = enemyGrid + dir;

            if (tilemap != null)
            {
                Vector3Int cellPos = new(targetGrid.x, targetGrid.y, 0);
                if (!tilemap.HasTile(cellPos))
                {
                    continue;
                }

            }

            if (obstacleTilemap != null && obstacleTilemap.IsTileObstacle(GridUtils.GridToWorld(targetGrid)))
            {
                continue;
            }

            Vector2 targetWorld = GridUtils.GridToWorld(targetGrid);
            StartCoroutine(MoveAlongPath(targetWorld));
            return;
        }
    }

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
            {
                return false;

            }

            Vector2 nextPos = path[1];
            Vector2 direction = (nextPos - (Vector2)transform.position).normalized;
            SetFacing(direction.x);

            StartCoroutine(MoveAlongPath(nextPos));
            return true;
        }
        return false;
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

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

    private void AttackPlayer()
    {
        if (playerComponent != null)
        {
            PlayerHealth playerHealth = playerComponent.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (audioSource != null && attackSound != null)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(attackSound);
                audioSource.pitch = 1f;
            }
        }
    }

    private void SetFacing(float xDirection)
    {
        if (Mathf.Abs(xDirection) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = xDirection < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
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
}