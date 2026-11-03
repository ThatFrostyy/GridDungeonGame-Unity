using UnityEngine;

/// <summary>
/// Moves away from the player when the enemy is low on health.
/// </summary>
public sealed class RetreatState : EnemyState
{
    public override string Name => "Retreat";

    public override bool ShouldEnter()
    {
        return Context.ShouldRetreat;
    }

    public override bool PerformAction()
    {
        if (!Context.CanAct)
        {
            return false;
        }

        Vector2Int current = Context.GridPosition;
        Vector2Int playerGrid = Context.PlayerGridPosition;
        Vector2Int bestTile = current;
        int bestDistance = GridUtils.ManhattanDistance(current, playerGrid);

        foreach (Vector2Int direction in GridUtils.CardinalDirections)
        {
            Vector2Int candidate = current + direction;
            int distance = GridUtils.ManhattanDistance(candidate, playerGrid);

            if (distance > bestDistance && Context.IsWalkable(candidate))
            {
                bestDistance = distance;
                bestTile = candidate;
            }
        }

        // When cornered this returns false and the state machine falls back to attacking.
        return bestTile != current && Context.TryMoveTo(bestTile);
    }
}
