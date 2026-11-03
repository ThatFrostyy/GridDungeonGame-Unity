using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Answers whether a tile is blocked. Queries the tilemap directly so tiles placed at runtime
/// (e.g. by the dungeon generator) are always taken into account.
/// </summary>
public class ObstacleTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap obstacleTilemap;

    public Tilemap Tilemap => obstacleTilemap;

    public bool IsTileObstacle(Vector2 worldPosition)
    {
        return obstacleTilemap.HasTile(obstacleTilemap.WorldToCell(worldPosition));
    }

    public bool IsTileObstacle(Vector2Int gridPosition)
    {
        return IsTileObstacle(GridUtils.GridToWorld(gridPosition));
    }
}
