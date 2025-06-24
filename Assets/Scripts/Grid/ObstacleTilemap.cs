using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ObstacleTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap obstacleTilemap;

    private readonly HashSet<Vector3Int> obstacleTilePositions = new();

    private void Awake()
    {
        InitializeObstacleTiles();
    }

    private void InitializeObstacleTiles()
    {
        obstacleTilePositions.Clear();

        BoundsInt bounds = obstacleTilemap.cellBounds;
        TileBase[] alltiles = obstacleTilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = alltiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Vector3Int tilePos = new(bounds.x + x, bounds.y + y, 0);
                    obstacleTilePositions.Add(tilePos);
                }
            }
        }
    }

    public bool IsTileObstacle(Vector2 position)
    {
        Vector3Int gridPos = obstacleTilemap.WorldToCell(position);

        return obstacleTilePositions.Contains(gridPos);
    }
}
