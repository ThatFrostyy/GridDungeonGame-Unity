using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks and runs the active state for an enemy AI turn.
/// States are evaluated in priority order; if the first applicable state cannot act
/// (e.g. a retreating enemy is cornered), the next applicable one gets a chance.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private string currentStateName = "None";

    private readonly List<EnemyState> states = new();
    private EnemyState currentState;
    private EnemyAIContext context;

    public string CurrentStateName => currentStateName;

    /// <summary>
    /// Configures the machine with priority-ordered states.
    /// </summary>
    public void Initialize(EnemyAIContext context, params EnemyState[] priorityOrderedStates)
    {
        this.context = context;
        states.Clear();
        currentState = null;
        currentStateName = "None";

        foreach (EnemyState state in priorityOrderedStates)
        {
            if (state == null)
            {
                continue;
            }

            state.Initialize(context);
            states.Add(state);
        }
    }

    /// <summary>
    /// Performs one action for the enemy turn. Returns false if no state could act.
    /// </summary>
    public bool PerformAction()
    {
        if (context == null || !context.CanAct)
        {
            return false;
        }

        foreach (EnemyState state in states)
        {
            if (!state.ShouldEnter())
            {
                continue;
            }

            ChangeState(state);
            if (state.PerformAction())
            {
                return true;
            }
        }

        return false;
    }

    private void ChangeState(EnemyState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentStateName = nextState.Name;
        currentState.Enter();
    }
}
