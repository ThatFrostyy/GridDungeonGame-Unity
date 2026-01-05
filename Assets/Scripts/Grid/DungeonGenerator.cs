using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
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

    private enum CellType
    {
        Empty,
        Floor,
        Wall
    }

    private struct Room
    {
        public RectInt bounds;
        public Vector2Int Center => Vector2Int.RoundToInt(bounds.center);
    }

    private CellType[,] dungeonGrid;
    private List<Room> rooms = new List<Room>();

    private Tilemap floorTilemap;
    private Tilemap obstacleTilemap;

    private void Awake()
    {
        InitializeReferences();

        InitializeGrid();
        GenerateRooms(roomCount);
        ConnectRooms();
        RemoveThinWalls();
        GenerateDungeon();
    }

    private void InitializeReferences()
    {
        if (GameObjectLocator.Instance != null)
        {
            floorTilemap = GameObjectLocator.Instance.Tilemap;
            obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemap.Tilemap;
        }
        else
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
        }
    }

    private void InitializeGrid()
    {
        dungeonGrid = new CellType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dungeonGrid[x, y] = CellType.Empty;
            }
        }
    }

    #region Setup Rooms

    private void GenerateRooms(int count)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dungeonGrid[x, y] = CellType.Wall;
            }
        }

        rooms.Clear();

        int maxAttempts = 50;

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int roomWidth = Random.Range(minRoomWidth, maxRoomWidth);
                int roomHeight = Random.Range(minRoomHeight, maxRoomHeight);

                int maxX = width - roomWidth - edgePadding;
                int maxY = height - roomHeight - edgePadding;

                if (maxX <= edgePadding || maxY <= edgePadding)
                {
                    Debug.LogWarning($"Dungeon too small for edge padding of {edgePadding}. " + $"Reduce padding or increase dungeon size.");
                    return;
                }

                int roomX = Random.Range(edgePadding, maxX);
                int roomY = Random.Range(edgePadding, maxY);

                RectInt newRoom = new RectInt(roomX, roomY, roomWidth, roomHeight);

                bool overlaps = false;
                foreach (var room in rooms)
                {
                    if (room.bounds.Overlaps(newRoom))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    rooms.Add(new Room { bounds = newRoom });

                    for (int x = newRoom.xMin; x < newRoom.xMax; x++)
                    {
                        for (int y = newRoom.yMin; y < newRoom.yMax; y++)
                        {
                            dungeonGrid[x, y] = CellType.Floor;
                        }
                    }

                    break;
                }
            }
        }
    }

    #endregion

    #region Connect Rooms

    private void ConnectRooms()
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int from = rooms[i - 1].Center;
            Vector2Int to = rooms[i].Center;

            bool horizontalFirst = Random.value > 0.5f;

            if (horizontalFirst)
            {
                HorizontalCorridor(from.x, to.x, from.y);
                VerticalCorridor(from.y, to.y, to.x);
            }
            else
            {
                VerticalCorridor(from.y, to.y, from.x);
                HorizontalCorridor(from.x, to.x, to.y);
            }
        }
    }

    private void HorizontalCorridor(int xStart, int xEnd, int y)
    {
        int width = Random.Range(minCorridorWidth, maxCorridorWidth + 1);
        int half = width / 2;

        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            for (int w = -half; w <= half; w++)
            {
                int corridorY = Mathf.Clamp(y + w, edgePadding, this.height - edgePadding - 1);
                dungeonGrid[x, corridorY] = CellType.Floor;
            }
        }
    }

    private void VerticalCorridor(int yStart, int yEnd, int x)
    {
        int width = Random.Range(minCorridorWidth, maxCorridorWidth + 1);
        int half = width / 2;

        for (int y = Mathf.Min(yStart, yEnd); y <= Mathf.Max(yStart, yEnd); y++)
        {
            for (int w = -half; w <= half; w++)
            {
                int corridorX = Mathf.Clamp(x + w, edgePadding, this.width - edgePadding - 1);
                dungeonGrid[corridorX, y] = CellType.Floor;
            }
        }
    }

    #endregion

    #region Post Process

    private void RemoveThinWalls()
    {
        List<Vector2Int> toFloor = new List<Vector2Int>();

        for (int x = edgePadding; x < width - edgePadding; x++)
        {
            for (int y = edgePadding; y < height - edgePadding; y++)
            {
                if (dungeonGrid[x, y] != CellType.Wall)
                    continue;

                bool floorLeft = dungeonGrid[x - 1, y] == CellType.Floor;
                bool floorRight = dungeonGrid[x + 1, y] == CellType.Floor;
                bool floorUp = dungeonGrid[x, y + 1] == CellType.Floor;
                bool floorDown = dungeonGrid[x, y - 1] == CellType.Floor;

                bool floorUpLeft = dungeonGrid[x - 1, y + 1] == CellType.Floor;
                bool floorUpRight = dungeonGrid[x + 1, y + 1] == CellType.Floor;
                bool floorDownLeft = dungeonGrid[x - 1, y - 1] == CellType.Floor;
                bool floorDownRight = dungeonGrid[x + 1, y - 1] == CellType.Floor;

                // Vertical thin wall
                if (floorLeft && floorRight)
                {
                    toFloor.Add(new Vector2Int(x, y));
                    continue;
                }

                // Horizontal thin wall
                if (floorUp && floorDown)
                {
                    toFloor.Add(new Vector2Int(x, y));
                    continue;
                }

                int floorCount = 0;

                if (floorLeft) floorCount++;
                if (floorRight) floorCount++;
                if (floorUp) floorCount++;
                if (floorDown) floorCount++;
                if (floorUpLeft) floorCount++;
                if (floorUpRight) floorCount++;
                if (floorDownLeft) floorCount++;
                if (floorDownRight) floorCount++;

                if (floorCount >= 6)
                {
                    toFloor.Add(new Vector2Int(x, y));
                    continue;
                }

                // Single cell wall
                if (floorLeft && floorRight && floorUp && floorDown)
                {
                    toFloor.Add(new Vector2Int(x, y));
                }
            }
        }

        foreach (var pos in toFloor)
        {
            dungeonGrid[pos.x, pos.y] = CellType.Floor;
        }
    }

    #endregion

    #region Tilemap Setup

    private void GenerateDungeon()
    {
        floorTilemap.ClearAllTiles();
        obstacleTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                switch (dungeonGrid[x, y])
                {
                    case CellType.Floor:
                        floorTilemap.SetTile(cellPos, floorTile);
                        break;

                    case CellType.Wall:
                        obstacleTilemap.SetTile(cellPos, wallTile);
                        break;
                }
            }
        }
    }

    #endregion
}
