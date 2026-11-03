using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Scene-level service locator that caches important references so other scripts
/// don't have to repeat tag lookups.
/// </summary>
public class GameObjectLocator : MonoBehaviour
{
    public static GameObjectLocator Instance { get; private set; }

    [Header("Cached References")]
    [SerializeField] private Player cachedPlayer;
    [SerializeField] private Tilemap cachedTilemap;
    [SerializeField] private Tilemap cachedObstacleTilemapMap;
    [SerializeField] private ObstacleTilemap cachedObstacleTilemap;
    [SerializeField] private TileSelection cachedTileSelection;
    [SerializeField] private UIManager cachedUIManager;

    private readonly Dictionary<string, GameObject> tagCache = new();
    private readonly Dictionary<(string tag, Type type), Component> componentCache = new();

    public Player Player => Resolve(ref cachedPlayer, "Player");
    public Tilemap Tilemap => Resolve(ref cachedTilemap, "Tilemap");
    public Tilemap ObstacleTilemapMap => Resolve(ref cachedObstacleTilemapMap, "ObstacleTilemap");
    public ObstacleTilemap ObstacleTilemap => Resolve(ref cachedObstacleTilemap, "ObstacleTilemap");
    public TileSelection TileSelection => Resolve(ref cachedTileSelection, "TileSelection");

    public UIManager UIManager
    {
        get
        {
            if (cachedUIManager == null)
            {
                cachedUIManager = FindFirstObjectByType<UIManager>();
            }

            return cachedUIManager;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        WarmUpReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Gets a GameObject by tag, caching the result.
    /// </summary>
    public GameObject GetGameObjectByTag(string tag)
    {
        if (tagCache.TryGetValue(tag, out GameObject cached) && cached != null)
        {
            return cached;
        }

        GameObject found = GameObject.FindGameObjectWithTag(tag);
        if (found != null)
        {
            tagCache[tag] = found;
        }

        return found;
    }

    /// <summary>
    /// Gets a component on the GameObject with the given tag, caching the result.
    /// </summary>
    public T GetComponentByTag<T>(string tag) where T : Component
    {
        var key = (tag, typeof(T));
        if (componentCache.TryGetValue(key, out Component cached) && cached != null)
        {
            return (T)cached;
        }

        GameObject target = GetGameObjectByTag(tag);
        if (target != null && target.TryGetComponent(out T component))
        {
            componentCache[key] = component;
            return component;
        }

        return null;
    }

    /// <summary>
    /// Clears all cached references (e.g. after the scene has been rebuilt).
    /// </summary>
    public void ClearCache()
    {
        tagCache.Clear();
        componentCache.Clear();
        cachedPlayer = null;
        cachedTilemap = null;
        cachedObstacleTilemapMap = null;
        cachedObstacleTilemap = null;
        cachedTileSelection = null;
        cachedUIManager = null;
    }

    /// <summary>
    /// Checks that an object with the given tag exists, optionally warning when it doesn't.
    /// </summary>
    public bool ValidateTagExists(string tag, bool logWarning = true)
    {
        bool exists = GetGameObjectByTag(tag) != null;

        if (!exists && logWarning)
        {
            Debug.LogWarning($"GameObject with tag '{tag}' not found. Make sure it exists in the scene.", this);
        }

        return exists;
    }

    private T Resolve<T>(ref T cache, string tag) where T : Component
    {
        if (cache == null)
        {
            cache = GetComponentByTag<T>(tag);
        }

        return cache;
    }

    private void WarmUpReferences()
    {
        _ = Player;
        _ = Tilemap;
        _ = ObstacleTilemapMap;
        _ = ObstacleTilemap;
        _ = TileSelection;
        _ = UIManager;

        ValidateTagExists("Player");
        ValidateTagExists("Tilemap");
        ValidateTagExists("ObstacleTilemap");
        ValidateTagExists("TileSelection");
    }
}
