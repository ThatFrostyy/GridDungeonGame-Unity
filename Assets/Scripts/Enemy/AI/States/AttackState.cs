/// <summary>
/// Attacks the player when they are within weapon range.
/// </summary>
public sealed class AttackState : EnemyState
{
    public override string Name => "Attack";

    public override bool ShouldEnter()
    {
        return Context != null && Context.IsPlayerInAttackRange;
    }

    public override bool PerformAction()
    {
        return Context != null && Context.TryAttackPlayer();
    }
}
