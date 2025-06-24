using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Action Points")]
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int currentActionPoints;

    [Header("References")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyHealthBar healthBar;

    public int MaxActionPoints => maxActionPoints;
    public int CurrentActionPoints => currentActionPoints;

    private void Awake()
    {
        currentActionPoints = maxActionPoints;

        if (health != null && healthBar != null)
        {
            healthBar.Setup(health, transform);
        }
    }

    public bool UseActionPoint(int amount = 1)
    {
        if (currentActionPoints >= amount)
        {
            currentActionPoints -= amount;
            return true;
        }
        return false;
    }

    public void GainActionPoints(int amount)
    {
        currentActionPoints = Mathf.Min(currentActionPoints + amount, maxActionPoints);
    }

    public void LoseActionPoints(int amount)
    {
        currentActionPoints = Mathf.Max(currentActionPoints - amount, 0);
    }

    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
    }
}
