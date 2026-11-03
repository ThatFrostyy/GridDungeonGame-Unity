/// <summary>
/// Moves toward the player when they are spotted but not in attack range.
/// </summary>
public sealed class ChaseState : EnemyState
{
    public override string Name => "Chase";

    public override bool ShouldEnter()
    {
        return Context.CanAct && !Context.IsPlayerOutsideSpotRange;
    }

    public override bool PerformAction()
    {
        return Context.CanAct && Context.TryStepTowardPlayer();
    }
}
