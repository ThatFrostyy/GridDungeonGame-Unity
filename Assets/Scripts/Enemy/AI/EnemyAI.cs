using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    /// <summary>
    /// Attempts to perform a single AI action.
    /// Returns true if an action was performed (and should consume an action point).
    /// </summary>
    public abstract bool PerformAI();
}