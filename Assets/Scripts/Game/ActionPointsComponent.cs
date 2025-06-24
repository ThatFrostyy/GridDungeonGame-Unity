using UnityEngine;
using System;

/// <summary>
/// Base component for handling action points in characters
/// Provides common functionality for player and enemies
/// </summary>
public abstract class ActionPointsComponent : MonoBehaviour
{
    [Header("Action Points Configuration")]
    [SerializeField] protected int maxActionPoints = 3;
    [SerializeField] protected int currentActionPoints;

    /// <summary>
    /// Maximum action points
    /// </summary>
    public int MaxActionPoints => maxActionPoints;

    /// <summary>
    /// Current action points
    /// </summary>
    public int CurrentActionPoints => currentActionPoints;

    /// <summary>
    /// Event triggered when action points change
    /// </summary>
    public event Action<int, int> OnActionPointsChanged;

    /// <summary>
    /// Initializes action points
    /// </summary>
    protected virtual void Awake()
    {
        InitializeActionPoints();
    }

    /// <summary>
    /// Initializes action points to maximum
    /// </summary>
    protected virtual void InitializeActionPoints()
    {
        currentActionPoints = maxActionPoints;
        NotifyActionPointsChanged();
    }

    /// <summary>
    /// Attempts to use action points
    /// </summary>
    /// <param name="amount">Amount of points to use</param>
    /// <returns>True if points could be used, false if not enough available</returns>
    public virtual bool UseActionPoint(int amount = 1)
    {
        if (!CanUseActionPoints(amount))
            return false;

        currentActionPoints -= amount;
        NotifyActionPointsChanged();
        OnActionPointUsed(amount);
        return true;
    }

    /// <summary>
    /// Checks if the specified amount of action points can be used
    /// </summary>
    /// <param name="amount">Amount to check</param>
    /// <returns>True if points can be used</returns>
    public virtual bool CanUseActionPoints(int amount)
    {
        return currentActionPoints >= amount && amount > 0;
    }

    /// <summary>
    /// Gains action points (without exceeding maximum)
    /// </summary>
    /// <param name="amount">Amount of points to gain</param>
    public virtual void GainActionPoints(int amount)
    {
        if (amount <= 0) return;

        currentActionPoints = Mathf.Min(currentActionPoints + amount, maxActionPoints);
        NotifyActionPointsChanged();
        OnActionPointsGained(amount);
    }

    /// <summary>
    /// Loses action points (without going below 0)
    /// </summary>
    /// <param name="amount">Amount of points to lose</param>
    public virtual void LoseActionPoints(int amount)
    {
        if (amount <= 0) return;

        currentActionPoints = Mathf.Max(currentActionPoints - amount, 0);
        NotifyActionPointsChanged();
        OnActionPointsLost(amount);
    }

    /// <summary>
    /// Resets action points to maximum
    /// </summary>
    public virtual void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
        NotifyActionPointsChanged();
        OnActionPointsReset();
    }

    /// <summary>
    /// Directly sets the amount of action points
    /// </summary>
    /// <param name="amount">New amount of points</param>
    public virtual void SetActionPoints(int amount)
    {
        currentActionPoints = Mathf.Clamp(amount, 0, maxActionPoints);
        NotifyActionPointsChanged();
    }

    /// <summary>
    /// Checks if has full action points
    /// </summary>
    public bool HasFullActionPoints => currentActionPoints == maxActionPoints;

    /// <summary>
    /// Checks if has no action points
    /// </summary>
    public bool HasNoActionPoints => currentActionPoints == 0;

    /// <summary>
    /// Gets the action points percentage (0.0 - 1.0)
    /// </summary>
    public float ActionPointsPercentage => (float)currentActionPoints / maxActionPoints;

    #region Protected Virtual Methods (For override in derived classes)
    
    /// <summary>
    /// Called when action points are used
    /// </summary>
    protected virtual void OnActionPointUsed(int amount) { }

    /// <summary>
    /// Called when action points are gained
    /// </summary>
    protected virtual void OnActionPointsGained(int amount) { }

    /// <summary>
    /// Called when action points are lost
    /// </summary>
    protected virtual void OnActionPointsLost(int amount) { }

    /// <summary>
    /// Called when action points are reset
    /// </summary>
    protected virtual void OnActionPointsReset() { }

    #endregion

    #region Private Methods
    
    /// <summary>
    /// Notifies action points change
    /// </summary>
    private void NotifyActionPointsChanged()
    {
        OnActionPointsChanged?.Invoke(currentActionPoints, maxActionPoints);
    }

    #endregion
} 