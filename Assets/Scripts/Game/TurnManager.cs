using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages turn-based flow between the player and enemies.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Delay between enemy actions.")]
    [SerializeField] private float enemyActionDelay = 0.5f;

    [Header("References")]
    [SerializeField] private Button endTurnButton;

    private Player player;
    private PlayerHealth playerHealth;
    private bool playerTurn = true;

    public static TurnManager Instance { get; private set; }

    public bool PlayerTurn => playerTurn;
    public float EnemyActionDelay => enemyActionDelay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        StartPlayerTurn();
    }

    private void InitializeReferences()
    {
        if (GameObjectLocator.Instance == null)
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
            return;
        }

        player = GameObjectLocator.Instance.Player;
        playerHealth = GameObjectLocator.Instance.GetComponentByTag<PlayerHealth>("Player");
    }

    /// <summary>
    /// Begins the player's turn: re-enables the end turn button and restores action points.
    /// </summary>
    public void StartPlayerTurn()
    {
        playerTurn = true;
        SetEndTurnInteractable(true);
        player.ResetActionPoints();
    }

    /// <summary>
    /// Ends the player's turn and runs every enemy's turn.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!playerTurn)
        {
            return;
        }

        playerTurn = false;
        SetEndTurnInteractable(false);
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        // Copy the list so enemies dying mid-turn don't invalidate the iteration.
        var enemies = new List<Enemy>(Enemy.Active);
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive)
            {
                continue;
            }

            enemy.ResetActionPoints();
            yield return RunEnemyActions(enemy);
            yield return new WaitForSeconds(enemyActionDelay);

            if (!IsPlayerAlive())
            {
                yield break;
            }
        }

        StartPlayerTurn();
    }

    private IEnumerator RunEnemyActions(Enemy enemy)
    {
        if (!enemy.TryGetComponent(out EnemyAI ai))
        {
            yield break;
        }

        while (enemy != null && enemy.IsAlive && enemy.CurrentActionPoints > 0 && IsPlayerAlive())
        {
            if (!ai.PerformAI())
            {
                yield break;
            }

            enemy.UseActionPoint();

            // Let movement finish before the next action starts.
            yield return new WaitUntil(() => ai == null || !ai.IsBusy);
            yield return new WaitForSeconds(enemyActionDelay);
        }
    }

    private bool IsPlayerAlive()
    {
        return playerHealth == null || playerHealth.IsAlive;
    }

    private void SetEndTurnInteractable(bool interactable)
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable;
        }
    }
}
