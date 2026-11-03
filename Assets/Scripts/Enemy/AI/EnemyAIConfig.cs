using UnityEngine;

/// <summary>
/// Read-only tuning values for an enemy AI, taken from the inspector settings.
/// </summary>
public readonly struct EnemyAIConfig
{
    public readonly float MoveSpeed;
    public readonly int SpotRange;
    public readonly float RetreatHealth;
    public readonly int AttackDamage;
    public readonly int AttackRange;
    public readonly float CritChance;
    public readonly float CritMultiplier;

    public EnemyAIConfig(
        float moveSpeed,
        int spotRange,
        float retreatHealth,
        int attackDamage,
        int attackRange,
        float critChance,
        float critMultiplier)
    {
        MoveSpeed = moveSpeed;
        SpotRange = spotRange;
        RetreatHealth = retreatHealth;
        AttackDamage = attackDamage;
        AttackRange = attackRange;
        CritChance = critChance;
        CritMultiplier = critMultiplier;
    }
}

/// <summary>
/// Sound effects an enemy AI can play.
/// </summary>
public readonly struct EnemyAIAudio
{
    public readonly AudioClip Move;
    public readonly AudioClip Attack;
    public readonly AudioClip Crit;

    public EnemyAIAudio(AudioClip move, AudioClip attack, AudioClip crit)
    {
        Move = move;
        Attack = attack;
        Crit = crit;
    }
}
