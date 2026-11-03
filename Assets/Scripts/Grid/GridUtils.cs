using UnityEngine;

/// <summary>
/// Conversions and helpers for the 1x1 world-unit tile grid.
/// </summary>
public static class GridUtils
{
    public static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

    public static Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    public static Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x + 0.5f, gridPos.y + 0.5f);
    }

    public static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return ManhattanDistance(a, b) == 1;
    }
}
