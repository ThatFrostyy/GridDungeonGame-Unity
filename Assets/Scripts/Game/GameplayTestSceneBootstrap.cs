using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Runtime setup for the dedicated gameplay test scene.
/// Keeps spawned actors on generated floor tiles after the dungeon is rebuilt.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(5000)]
public class GameplayTestSceneBootstrap : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector2Int preferredPlayerGrid = new(3, 3);
    [SerializeField] private Vector2Int preferredEnemyGrid = new(8, 3);
    [SerializeField] private int searchRadius = 30;

    [Header("Camera")]
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Vector3 cameraOffset = new(0f, 0f, -10f);
    [SerializeField] private float cameraOrthographicSize = 12f;

    private void Start()
    {
        PrepareScene();
    }

    private void PrepareScene()
    {
        GameObjectLocator locator = GameObjectLocator.Instance;
        if (locator == null)
        {
            Debug.LogError("Gameplay test scene is missing a GameObjectLocator.", this);
            return;
        }

        Tilemap floorTilemap = locator.Tilemap;
        ObstacleTilemap obstacleTilemap = locator.ObstacleTilemap;
        Player player = locator.Player;

        if (floorTilemap == null || player == null)
        {
            Debug.LogError("Gameplay test scene needs a tagged floor Tilemap and Player prefab.", this);
            return;
        }

        obstacleTilemap?.RefreshObstacleTiles();

        List<Vector2Int> reservedTiles = new();
        Vector2Int playerTile = preferredPlayerGrid;
        if (TryFindWalkableTile(floorTilemap, obstacleTilemap, preferredPlayerGrid, reservedTiles, out playerTile))
        {
            MoveTransformToGrid(player.transform, playerTile);
            reservedTiles.Add(playerTile);
        }
        else
        {
            Debug.LogWarning("Could not find a generated floor tile for the player.", this);
        }

        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        for (int i = 0; i < enemies.Length; i++)
        {
            Vector2Int desiredEnemyTile = preferredEnemyGrid + new Vector2Int(i * 2, 0);
            if (TryFindWalkableTile(floorTilemap, obstacleTilemap, desiredEnemyTile, reservedTiles, out Vector2Int enemyTile))
            {
                MoveTransformToGrid(enemies[i].transform, enemyTile);
                reservedTiles.Add(enemyTile);
            }
        }

        FrameCamera(player.transform);
        Debug.Log($"Gameplay test scene ready. Player tile: {playerTile}. Enemy count: {enemies.Length}.", this);
    }

    private bool TryFindWalkableTile(
        Tilemap floorTilemap,
        ObstacleTilemap obstacleTilemap,
        Vector2Int preferredTile,
        List<Vector2Int> reservedTiles,
        out Vector2Int result)
    {
        if (IsWalkable(floorTilemap, obstacleTilemap, preferredTile, reservedTiles))
        {
            result = preferredTile;
            return true;
        }

        for (int radius = 1; radius <= searchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                    {
                        continue;
                    }

                    Vector2Int candidate = preferredTile + new Vector2Int(x, y);
                    if (IsWalkable(floorTilemap, obstacleTilemap, candidate, reservedTiles))
                    {
                        result = candidate;
                        return true;
                    }
                }
            }
        }

        foreach (Vector3Int cell in floorTilemap.cellBounds.allPositionsWithin)
        {
            Vector2Int candidate = new(cell.x, cell.y);
            if (IsWalkable(floorTilemap, obstacleTilemap, candidate, reservedTiles))
            {
                result = candidate;
                return true;
            }
        }

        result = preferredTile;
        return false;
    }

    private static bool IsWalkable(
        Tilemap floorTilemap,
        ObstacleTilemap obstacleTilemap,
        Vector2Int tile,
        List<Vector2Int> reservedTiles)
    {
        if (reservedTiles.Contains(tile))
        {
            return false;
        }

        Vector3Int cell = new(tile.x, tile.y, 0);
        if (!floorTilemap.HasTile(cell))
        {
            return false;
        }

        return obstacleTilemap == null || !obstacleTilemap.IsTileObstacle(GridUtils.GridToWorld(tile));
    }

    private static void MoveTransformToGrid(Transform target, Vector2Int gridPosition)
    {
        Vector2 worldPosition = GridUtils.GridToWorld(gridPosition);
        target.position = new Vector3(worldPosition.x, worldPosition.y, target.position.z);
    }

    private void FrameCamera(Transform target)
    {
        Camera cameraToFrame = sceneCamera != null ? sceneCamera : Camera.main;
        if (cameraToFrame == null || target == null)
        {
            return;
        }

        cameraToFrame.orthographic = true;
        cameraToFrame.orthographicSize = cameraOrthographicSize;
        cameraToFrame.transform.position = target.position + cameraOffset;
    }
}
