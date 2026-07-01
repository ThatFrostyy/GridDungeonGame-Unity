using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects and runs the active state for an enemy AI turn.
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
    /// Configures this state machine with priority-ordered states.
    /// </summary>
    public void Initialize(EnemyAIContext context, params EnemyState[] states)
    {
        this.context = context;
        this.states.Clear();
        currentState = null;
        currentStateName = "None";

        foreach (EnemyState state in states)
        {
            if (state == null)
            {
                continue;
            }

            state.Initialize(context);
            this.states.Add(state);
        }
    }

    /// <summary>
    /// Selects a state and performs one action for the enemy turn.
    /// </summary>
    public bool PerformAction()
    {
        if (context == null || !context.CanAct)
        {
            return false;
        }

        EnemyState nextState = GetNextState();
        if (nextState == null)
        {
            return false;
        }

        ChangeState(nextState);
        return currentState.PerformAction();
    }

    private EnemyState GetNextState()
    {
        foreach (EnemyState state in states)
        {
            if (state.ShouldEnter())
            {
                return state;
            }
        }

        return null;
    }

    private void ChangeState(EnemyState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentStateName = currentState.Name;
        currentState.Enter();
    }
}
