using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSelection : MonoBehaviour
{   
    [SerializeField] private Vector2 gridSize = new(1, 1);

    private Vector2Int highlightedTilePosition = Vector2Int.zero;

    private Tilemap tilemap;
    private Tilemap obstacleTilemap;

    private void Awake()
    {
        GameObject tilemapObject = GameObject.FindGameObjectWithTag("Tilemap");
        GameObject obstacleTilemapObject = GameObject.FindGameObjectWithTag("ObstacleTilemap");
        if (tilemapObject != null)
        {
            tilemap = tilemapObject.GetComponent<Tilemap>();
        }
        else
        {
             Debug.LogWarning("Tilemap not found. Ensure a Tilemap with the 'Tilemap' tag exists in the scene.");
        }
        if (obstacleTilemapObject != null)
        {
            obstacleTilemap = obstacleTilemapObject.GetComponent<Tilemap>();
        }
        else
        {
            Debug.LogWarning("ObstacleTilemap not found. Ensure a Tilemap with the 'ObstacleTilemap' tag exists in the scene.");
        }
    }

    private void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new(
            Mathf.FloorToInt(mouseWorldPos.x / gridSize.x) * Mathf.RoundToInt(gridSize.x),
            Mathf.FloorToInt(mouseWorldPos.y / gridSize.y) * Mathf.RoundToInt(gridSize.y)
        );

        bool isObstacleTile = false;
        if (obstacleTilemap != null)
        {
            Vector3Int cellPos = obstacleTilemap.WorldToCell(mouseWorldPos);
            if (obstacleTilemap.HasTile(cellPos) && obstacleTilemap.GetTile(cellPos) != null)
            {
                isObstacleTile = true;
            }
        }

        bool isNormalTile = false;
        if (tilemap != null)
        {
            Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
            if (tilemap.HasTile(cellPos) && tilemap.GetTile(cellPos) != null)
            {
                isNormalTile = true;
            }
        }

        if (!isObstacleTile && isNormalTile)
        {
            highlightedTilePosition = gridPos;
            Vector2 worldPos = GridUtils.GridToWorld(gridPos);
            transform.position = worldPos;
        }
    }

    public Vector2Int HighlightedTilePosition
    {
        get { return highlightedTilePosition; }
    }

    public bool IsHighlightedTileClicked(Vector2 clickedPosition)
    {
        Vector2Int gridPos = GridUtils.WorldToGrid(clickedPosition);
        return gridPos == highlightedTilePosition;
    }

    public Vector2 GetHighlightedTileWorldPosition()
    {
        return GridUtils.GridToWorld(highlightedTilePosition);
    }

    public bool IsTileObstacle(Vector2Int position)
    {
        Vector3 worldPos = GridUtils.GridToWorld(position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }
}
