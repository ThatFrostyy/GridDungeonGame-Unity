using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages turn-based flow between player and enemies, including turn transitions and enemy AI actions.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Delay between enemy actions.")]
    [SerializeField] private float enemyActionDelay = 0.5f; 

    [Header("References")]
    [SerializeField] private Button endTurnButton;

    private Enemy[] enemies;
    private Player player;

    private bool playerTurn = true;

    public bool PlayerTurn => playerTurn;
    public float EnemyActionDelay => enemyActionDelay;

    public static TurnManager Instance { get; private set; }

    /// <summary>
    /// Initializes references using the GameObjectLocator
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeReferences();
    }

    /// <summary>
    /// Gets necessary references from the GameObjectLocator
    /// </summary>
    private void InitializeReferences()
    {
        if (GameObjectLocator.Instance != null)
        {
            player = GameObjectLocator.Instance.Player;

        }
        else
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
        }

        enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        StartPlayerTurn();
    }

    /// <summary>
    /// Begins the player's turn, enabling the end turn button and resetting player action points.
    /// </summary>
    public void StartPlayerTurn()
    {
        playerTurn = true;
        endTurnButton.interactable = true;
        player.ResetActionPoints();

    }

    /// <summary>
    /// Ends the player's turn, disables the end turn button, resets enemy action points, and starts the enemy turn.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (playerTurn)
        {
            playerTurn = false;
            endTurnButton.interactable = false;
            foreach (var enemy in enemies)
            {
                enemy.ResetActionPoints();
            }
            StartCoroutine(EnemyTurn());
        }
    }

    /// <summary>
    /// Coroutine that processes each enemy's turn, executing their AI actions with delays.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator EnemyTurn()
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null) 
                continue;

            if (enemy.TryGetComponent<EnemyAI>(out var ai))
            {
                while (enemy.CurrentActionPoints > 0)
                {
                    bool actionDone = ai.PerformAI();
                    if (actionDone)
                    {
                        enemy.UseActionPoint();
                        yield return new WaitForSeconds(enemyActionDelay);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(enemyActionDelay);
        }

        StartPlayerTurn();
    }
}