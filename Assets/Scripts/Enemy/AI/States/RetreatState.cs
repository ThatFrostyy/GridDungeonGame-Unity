/// <summary>
/// Moves away from the player when the enemy is low on health.
/// </summary>
public sealed class RetreatState : EnemyState
{
    public override string Name => "Retreat";

    public override bool ShouldEnter()
    {
        return Context != null && Context.ShouldRetreat;
    }

    public override bool PerformAction()
    {
        return Context != null && Context.TryRetreat();
    }
}
