using UnityEngine;

public static class GridUtils
{
    public static Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    public static Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x + 0.5f, gridPos.y + 0.5f);
    }
}