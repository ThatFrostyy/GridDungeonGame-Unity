using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    /// <summary>
    /// True while the enemy is still carrying out its last action (e.g. walking to a tile).
    /// </summary>
    public virtual bool IsBusy => false;

    /// <summary>
    /// Attempts to perform a single AI action.
    /// Returns true if an action was performed (and should consume an action point).
    /// </summary>
    public abstract bool PerformAI();
}
