using UnityEngine;

/// <summary>
/// Cached animator parameter hashes shared by the player and enemies.
/// </summary>
public static class AnimatorParams
{
    public static readonly int IsWalking = Animator.StringToHash("isWalking");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int WeaponType = Animator.StringToHash("WeaponType");
}
