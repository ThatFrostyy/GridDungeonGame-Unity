using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Moves a highlight marker to the walkable tile under the mouse cursor.
/// </summary>
public class TileSelection : MonoBehaviour
{
    private Vector2Int highlightedTilePosition = Vector2Int.zero;

    private Tilemap tilemap;
    private Tilemap obstacleTilemap;
    private Camera mainCamera;

    public Vector2Int HighlightedTilePosition => highlightedTilePosition;

    // Start (not Awake) so the GameObjectLocator is guaranteed to exist.
    private void Start()
    {
        mainCamera = Camera.main;

        if (GameObjectLocator.Instance == null)
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
            return;
        }

        tilemap = GameObjectLocator.Instance.Tilemap;
        obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemapMap;

        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap not found. Ensure a Tilemap with the 'Tilemap' tag exists in the scene.", this);
        }

        if (obstacleTilemap == null)
        {
            Debug.LogWarning("ObstacleTilemap not found. Ensure a Tilemap with the 'ObstacleTilemap' tag exists in the scene.", this);
        }
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridUtils.WorldToGrid(mouseWorldPos);

        if (HasTileAt(tilemap, mouseWorldPos) && !HasTileAt(obstacleTilemap, mouseWorldPos))
        {
            highlightedTilePosition = gridPos;
            transform.position = GridUtils.GridToWorld(gridPos);
        }
    }

    public Vector2 GetHighlightedTileWorldPosition()
    {
        return GridUtils.GridToWorld(highlightedTilePosition);
    }

    private static bool HasTileAt(Tilemap map, Vector2 worldPosition)
    {
        return map != null && map.HasTile(map.WorldToCell(worldPosition));
    }
}
