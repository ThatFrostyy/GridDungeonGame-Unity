using UnityEngine;

/// <summary>
/// Attacks the player when they are within weapon range.
/// </summary>
public sealed class AttackState : EnemyState
{
    public override string Name => "Attack";

    public override bool ShouldEnter()
    {
        return Context.IsPlayerInAttackRange;
    }

    public override bool PerformAction()
    {
        if (!Context.CanAct || Context.PlayerHealth == null || !Context.IsPlayerInAttackRange)
        {
            return false;
        }

        EnemyAIConfig config = Context.Config;
        Context.FacePosition(Context.PlayerPosition);

        int damage = config.AttackDamage;
        if (Random.value < config.CritChance)
        {
            damage = Mathf.RoundToInt(damage * config.CritMultiplier);
            Context.PlaySound(Context.Audio.Crit);
        }
        else
        {
            Context.PlaySound(Context.Audio.Attack, randomizePitch: true);
        }

        Context.PlayerHealth.TakeDamage(damage);
        Context.TriggerAttackAnimation();
        return true;
    }
}
