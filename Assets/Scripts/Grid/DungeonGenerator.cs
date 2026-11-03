using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates a room-and-corridor dungeon and paints it onto the floor and obstacle tilemaps.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    private const int RoomPlacementAttempts = 50;

    [Header("Dungeon Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private int roomCount = 8;
    [SerializeField] private int edgePadding = 2;

    [Header("Room Settings")]
    [SerializeField] private int minRoomWidth = 4;
    [SerializeField] private int maxRoomWidth = 10;
    [SerializeField] private int minRoomHeight = 4;
    [SerializeField] private int maxRoomHeight = 10;

    [Header("Corridor Settings")]
    [SerializeField] private int minCorridorWidth = 1;
    [SerializeField] private int maxCorridorWidth = 3;

    [Header("Tiles")]
    [SerializeField] private RuleTile floorTile;
    [SerializeField] private RuleTile wallTile;

    [Header("Spawning")]
    [Tooltip("Enemies are placed at least this many tiles (Manhattan) away from the player's start.")]
    [SerializeField] private int minEnemySpawnDistance = 6;

    [Header("Seed")]
    [Tooltip("Generate the same dungeon every time.")]
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int seed = 0;

    private enum CellType
    {
        Empty,
        Floor,
        Wall
    }

    private readonly List<RectInt> rooms = new();
    private CellType[,] grid;
    private System.Random random;
    private Tilemap floorTilemap;
    private Tilemap obstacleTilemap;

    /// <summary>
    /// Bounds of every generated room, in tile coordinates.
    /// </summary>
    public IReadOnlyList<RectInt> Rooms => rooms;

    private void Awake()
    {
        Generate();
    }

    // Start (not Awake) so the player and every enemy have been enabled and registered.
    private void Start()
    {
        PlaceActors();
    }

    /// <summary>
    /// Builds a new dungeon and repaints the tilemaps.
    /// </summary>
    [ContextMenu("Regenerate")]
    public void Generate()
    {
        if (floorTilemap == null && !InitializeReferences())
        {
            return;
        }

        random = useFixedSeed ? new System.Random(seed) : new System.Random();

        FillGrid(CellType.Wall);
        GenerateRooms();
        ConnectRooms();
        RemoveThinWalls();
        PaintTilemaps();
    }

    private bool InitializeReferences()
    {
        if (GameObjectLocator.Instance == null)
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
            return false;
        }

        floorTilemap = GameObjectLocator.Instance.Tilemap;
        obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemap.Tilemap;
        return true;
    }

    private void FillGrid(CellType cellType)
    {
        grid = new CellType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = cellType;
            }
        }
    }

    // Random.Range-style helper: min inclusive, max exclusive.
    private int Next(int minInclusive, int maxExclusive)
    {
        return random.Next(minInclusive, maxExclusive);
    }

    #region Spawning

    /// <summary>
    /// Puts the player in the center of the first room and scatters the enemies over the floor,
    /// away from the player.
    /// </summary>
    private void PlaceActors()
    {
        if (rooms.Count == 0)
        {
            return;
        }

        Vector2Int playerSpawn = Vector2Int.RoundToInt(rooms[0].center);

        Player player = GameObjectLocator.Instance != null ? GameObjectLocator.Instance.Player : null;
        if (player != null)
        {
            MoveToTile(player.transform, playerSpawn);
        }

        List<Vector2Int> candidates = FindEnemySpawnTiles(playerSpawn);
        foreach (Enemy enemy in new List<Enemy>(Enemy.Active))
        {
            if (candidates.Count == 0)
            {
                Debug.LogWarning("Not enough free tiles to place every enemy.", this);
                return;
            }

            int index = Next(0, candidates.Count);
            MoveToTile(enemy.transform, candidates[index]);

            // Swap-remove keeps this O(1) and guarantees no two enemies share a tile.
            candidates[index] = candidates[candidates.Count - 1];
            candidates.RemoveAt(candidates.Count - 1);
        }
    }

    private List<Vector2Int> FindEnemySpawnTiles(Vector2Int playerSpawn)
    {
        var tiles = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = new Vector2Int(x, y);
                if (grid[x, y] == CellType.Floor && GridUtils.ManhattanDistance(tile, playerSpawn) >= minEnemySpawnDistance)
                {
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }

    private static void MoveToTile(Transform target, Vector2Int tile)
    {
        Vector2 world = GridUtils.GridToWorld(tile);
        target.position = new Vector3(world.x, world.y, target.position.z);
    }

    #endregion

    #region Rooms

    private void GenerateRooms()
    {
        rooms.Clear();

        for (int i = 0; i < roomCount; i++)
        {
            if (!TryPlaceRoom(out RectInt room))
            {
                continue;
            }

            rooms.Add(room);
            CarveRoom(room);
        }
    }

    private bool TryPlaceRoom(out RectInt room)
    {
        for (int attempt = 0; attempt < RoomPlacementAttempts; attempt++)
        {
            int roomWidth = Next(minRoomWidth, maxRoomWidth + 1);
            int roomHeight = Next(minRoomHeight, maxRoomHeight + 1);

            int maxX = width - roomWidth - edgePadding;
            int maxY = height - roomHeight - edgePadding;

            if (maxX <= edgePadding || maxY <= edgePadding)
            {
                continue;
            }

            room = new RectInt(Next(edgePadding, maxX), Next(edgePadding, maxY), roomWidth, roomHeight);
            if (!OverlapsExistingRoom(room))
            {
                return true;
            }
        }

        room = default;
        return false;
    }

    private bool OverlapsExistingRoom(RectInt candidate)
    {
        foreach (RectInt existing in rooms)
        {
            if (existing.Overlaps(candidate))
            {
                return true;
            }
        }

        return false;
    }

    private void CarveRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                grid[x, y] = CellType.Floor;
            }
        }
    }

    #endregion

    #region Corridors

    private void ConnectRooms()
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int from = Vector2Int.RoundToInt(rooms[i - 1].center);
            Vector2Int to = Vector2Int.RoundToInt(rooms[i].center);

            if (random.NextDouble() > 0.5)
            {
                CarveHorizontalCorridor(from.x, to.x, from.y);
                CarveVerticalCorridor(from.y, to.y, to.x);
            }
            else
            {
                CarveVerticalCorridor(from.y, to.y, from.x);
                CarveHorizontalCorridor(from.x, to.x, to.y);
            }
        }
    }

    private void CarveHorizontalCorridor(int xStart, int xEnd, int y)
    {
        int half = RandomCorridorWidth() / 2;

        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            for (int offset = -half; offset <= half; offset++)
            {
                int corridorY = Mathf.Clamp(y + offset, edgePadding, height - edgePadding - 1);
                grid[x, corridorY] = CellType.Floor;
            }
        }
    }

    private void CarveVerticalCorridor(int yStart, int yEnd, int x)
    {
        int half = RandomCorridorWidth() / 2;

        for (int y = Mathf.Min(yStart, yEnd); y <= Mathf.Max(yStart, yEnd); y++)
        {
            for (int offset = -half; offset <= half; offset++)
            {
                int corridorX = Mathf.Clamp(x + offset, edgePadding, width - edgePadding - 1);
                grid[corridorX, y] = CellType.Floor;
            }
        }
    }

    private int RandomCorridorWidth()
    {
        return Next(minCorridorWidth, maxCorridorWidth + 1);
    }

    #endregion

    #region Post Process

    /// <summary>
    /// Turns walls that are only one tile thick (or almost fully surrounded by floor) into floor.
    /// </summary>
    private void RemoveThinWalls()
    {
        var toFloor = new List<Vector2Int>();

        // Stay one tile away from the border so every neighbour lookup is in range.
        int inset = Mathf.Max(edgePadding, 1);

        for (int x = inset; x < width - inset; x++)
        {
            for (int y = inset; y < height - inset; y++)
            {
                if (grid[x, y] == CellType.Wall && IsThinWall(x, y))
                {
                    toFloor.Add(new Vector2Int(x, y));
                }
            }
        }

        foreach (Vector2Int position in toFloor)
        {
            grid[position.x, position.y] = CellType.Floor;
        }
    }

    private bool IsThinWall(int x, int y)
    {
        bool floorLeft = grid[x - 1, y] == CellType.Floor;
        bool floorRight = grid[x + 1, y] == CellType.Floor;
        bool floorUp = grid[x, y + 1] == CellType.Floor;
        bool floorDown = grid[x, y - 1] == CellType.Floor;

        if ((floorLeft && floorRight) || (floorUp && floorDown))
        {
            return true;
        }

        int floorNeighbours = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if ((dx != 0 || dy != 0) && grid[x + dx, y + dy] == CellType.Floor)
                {
                    floorNeighbours++;
                }
            }
        }

        return floorNeighbours >= 6;
    }

    #endregion

    #region Tilemaps

    private void PaintTilemaps()
    {
        floorTilemap.ClearAllTiles();
        obstacleTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = new Vector3Int(x, y, 0);

                switch (grid[x, y])
                {
                    case CellType.Floor:
                        floorTilemap.SetTile(cell, floorTile);
                        break;

                    case CellType.Wall:
                        obstacleTilemap.SetTile(cell, wallTile);
                        break;
                }
            }
        }
    }

    #endregion
}
