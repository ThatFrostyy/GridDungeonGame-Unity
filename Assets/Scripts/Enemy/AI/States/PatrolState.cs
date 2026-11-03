using UnityEngine;

/// <summary>
/// Randomly wanders while the player is outside spotting range.
/// </summary>
public sealed class PatrolState : EnemyState
{
    public override string Name => "Patrol";

    public override bool ShouldEnter()
    {
        return Context.IsPlayerOutsideSpotRange;
    }

    public override bool PerformAction()
    {
        if (!Context.CanAct)
        {
            return false;
        }

        // Try every direction once, starting from a random one.
        Vector2Int[] directions = GridUtils.CardinalDirections;
        int start = Random.Range(0, directions.Length);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int direction = directions[(start + i) % directions.Length];
            if (Context.TryMoveTo(Context.GridPosition + direction))
            {
                return true;
            }
        }

        return false;
    }
}
