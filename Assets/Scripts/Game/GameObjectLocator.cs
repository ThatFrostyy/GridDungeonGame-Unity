using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that centralizes GameObject location by tag
/// Avoids multiple FindGameObjectWithTag calls and caches important references
/// </summary>
public class GameObjectLocator : MonoBehaviour
{
    #region Singleton
    public static GameObjectLocator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeReferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Cached References
    [Header("Cached References")]
    [SerializeField] private Player cachedPlayer;
    [SerializeField] private ObstacleTilemap cachedObstacleTilemap;
    [SerializeField] private TileSelection cachedTileSelection;
    [SerializeField] private UIManager cachedUIManager;

    private readonly Dictionary<string, GameObject> _tagCache = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, Component> _componentCache = new Dictionary<string, Component>();
    #endregion

    #region Public Properties
    public Player Player
    {
        get
        {
            if (cachedPlayer == null)
                cachedPlayer = GetComponentByTag<Player>("Player");
            return cachedPlayer;
        }
    }

    public ObstacleTilemap ObstacleTilemap
    {
        get
        {
            if (cachedObstacleTilemap == null)
                cachedObstacleTilemap = GetComponentByTag<ObstacleTilemap>("ObstacleTilemap");
            return cachedObstacleTilemap;
        }
    }

    public TileSelection TileSelection
    {
        get
        {
            if (cachedTileSelection == null)
                cachedTileSelection = GetComponentByTag<TileSelection>("TileSelection");
            return cachedTileSelection;
        }
    }

    public UIManager UIManager
    {
        get
        {
            if (cachedUIManager == null)
                cachedUIManager = FindObjectOfType<UIManager>();
            return cachedUIManager;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Gets a GameObject by tag with caching
    /// </summary>
    /// <param name="tag">Tag of the GameObject to search for</param>
    /// <returns>Found GameObject or null</returns>
    public GameObject GetGameObjectByTag(string tag)
    {
        if (_tagCache.TryGetValue(tag, out GameObject cachedObject) && cachedObject != null)
        {
            return cachedObject;
        }

        GameObject foundObject = GameObject.FindGameObjectWithTag(tag);
        if (foundObject != null)
        {
            _tagCache[tag] = foundObject;
        }

        return foundObject;
    }

    /// <summary>
    /// Gets a component by tag with caching
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <param name="tag">Tag of the GameObject containing the component</param>
    /// <returns>Found component or null</returns>
    public T GetComponentByTag<T>(string tag) where T : Component
    {
        string key = $"{tag}_{typeof(T).Name}";

        if (_componentCache.TryGetValue(key, out Component cachedComponent) && cachedComponent != null)
        {
            return cachedComponent as T;
        }

        GameObject gameObject = GetGameObjectByTag(tag);
        if (gameObject != null)
        {
            T component = gameObject.GetComponent<T>();
            if (component != null)
            {
                _componentCache[key] = component;
            }
            return component;
        }

        return null;
    }

    /// <summary>
    /// Clears the caches (useful when objects are destroyed)
    /// </summary>
    public void ClearCache()
    {
        _tagCache.Clear();
        _componentCache.Clear();
        cachedPlayer = null;
        cachedObstacleTilemap = null;
        cachedTileSelection = null;
        cachedUIManager = null;
    }

    /// <summary>
    /// Validates that an object with the specified tag exists
    /// </summary>
    /// <param name="tag">Tag to validate</param>
    /// <param name="logWarning">Whether to show warning if object is not found</param>
    /// <returns>True if exists, false if not</returns>
    public bool ValidateTagExists(string tag, bool logWarning = true)
    {
        GameObject obj = GetGameObjectByTag(tag);
        bool exists = obj != null;

        if (!exists && logWarning)
        {
            Debug.LogWarning($"GameObject with tag '{tag}' not found. Make sure it exists in the scene.", this);
        }

        return exists;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes references on awake
    /// </summary>
    private void InitializeReferences()
    {
        // Pre-load important references
        _ = Player;
        _ = ObstacleTilemap;
        _ = TileSelection;
        _ = UIManager;

        // Validate that critical objects exist
        ValidateTagExists("Player");
        ValidateTagExists("ObstacleTilemap");
        ValidateTagExists("TileSelection");
    }
    #endregion
} 