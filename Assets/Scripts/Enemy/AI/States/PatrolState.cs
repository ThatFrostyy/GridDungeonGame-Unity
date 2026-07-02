/// <summary>
/// Randomly moves while the player is outside spotting range.
/// </summary>
public sealed class PatrolState : EnemyState
{
    public override string Name => "Patrol";

    public override bool ShouldEnter()
    {
        return Context != null && Context.IsPlayerOutsideSpotRange;
    }

    public override bool PerformAction()
    {
        return Context != null && Context.TryPatrol();
    }
}
