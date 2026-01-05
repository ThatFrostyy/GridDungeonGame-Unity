using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator2D : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private int roomCount = 8;

    [Header("Room Settings")]
    [SerializeField] private int minRoomWidth = 4;
    [SerializeField] private int maxRoomWidth = 10;
    [SerializeField] private int minRoomHeight = 4;
    [SerializeField] private int maxRoomHeight = 10;

    [Header("Corridor Settings")]
    [SerializeField] private int minCorridorWidth = 1;
    [SerializeField] private int maxCorridorWidth = 3;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile; // can be RuleTile

    private CellType[,] dungeonGrid;

    private struct Room
    {
        public RectInt bounds;
        public Vector2Int Center => Vector2Int.RoundToInt(bounds.center);
    }

    private List<Room> rooms = new List<Room>();

    private Tilemap floorTilemap;
    private Tilemap obstacleTilemap;

    private void Awake()
    {
        InitializeReferences();
    }

    private void Start()
    {
        InitializeGrid();
        GenerateRooms(roomCount);
        ConnectRooms();
        GenerateDungeon();
    }

    private void InitializeReferences()
    {
        if (GameObjectLocator.Instance != null)
        {
            obstacleTilemap = GameObjectLocator.Instance.ObstacleTilemap.Tilemap;
            floorTilemap = GameObjectLocator.Instance.Tilemap;
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

        for (int i = 0; i < count; i++)
        {
            int roomWidth = Random.Range(minRoomWidth, maxRoomWidth);
            int roomHeight = Random.Range(minRoomHeight, maxRoomHeight);

            int roomX = Random.Range(1, width - roomWidth - 1);
            int roomY = Random.Range(1, height - roomHeight - 1);

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

            if (overlaps)
            {
                i--;
                continue;
            }

            rooms.Add(new Room { bounds = newRoom });

            for (int x = newRoom.xMin; x < newRoom.xMax; x++)
            {
                for (int y = newRoom.yMin; y < newRoom.yMax; y++)
                {
                    dungeonGrid[x, y] = CellType.Floor;
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
                dungeonGrid[x, y + w] = CellType.Floor;
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
                dungeonGrid[x + w, y] = CellType.Floor;
            }
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

public enum CellType
{
    Empty,
    Floor,
    Wall
}