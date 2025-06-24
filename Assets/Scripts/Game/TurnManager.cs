using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float enemyActionDelay = 0.5f; 

    [Header("References")]
    [SerializeField] private Button endTurnButton;

    private Enemy[] enemies;
    private Player player;
    private bool playerTurn = true;

    public bool PlayerTurn => playerTurn;
    public float EnemyActionDelay => enemyActionDelay;

    public static TurnManager Instance { get; private set; }

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

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player object not found. Ensure the player has the 'Player' tag.");
        }

        enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        playerTurn = true;
        endTurnButton.interactable = true;
        player.ResetActionPoints();

    }

    public void EndPlayerTurn()
    {
        playerTurn = false;
        endTurnButton.interactable = false;
        foreach (var enemy in enemies)
        {
            enemy.ResetActionPoints();
        }
        StartCoroutine(EnemyTurn());
    }

    public void OnEndTurnButtonPressed()
    {
        if (playerTurn)
        {
            EndPlayerTurn();
        }
    }

    private IEnumerator EnemyTurn()
    {
        bool anyEnemyActed = false;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            if (enemy.TryGetComponent<EnemyAI>(out var ai))
            {
                while (enemy.CurrentActionPoints > 0)
                {
                    ai.PerformAI();
                    yield return new WaitForSeconds(enemyActionDelay);
                }

                anyEnemyActed = true;
            }

            yield return new WaitForSeconds(enemyActionDelay); 
        }

        if (!anyEnemyActed)
        {
            yield return new WaitForSeconds(enemyActionDelay);
        }

        StartPlayerTurn();
    }

}