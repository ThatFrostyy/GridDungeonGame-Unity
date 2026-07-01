/// <summary>
/// Moves toward the player when they are spotted but not in attack range.
/// </summary>
public sealed class ChaseState : EnemyState
{
    public override string Name => "Chase";

    public override bool ShouldEnter()
    {
        return Context != null && !Context.IsPlayerOutsideSpotRange;
    }

    public override bool PerformAction()
    {
        return Context != null && Context.TryChasePlayer();
    }
}
