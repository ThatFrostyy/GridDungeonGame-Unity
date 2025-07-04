using UnityEngine;

/// <summary>
/// Handles player health, extends the base Health functionality
/// </summary>
public class PlayerHealth : Health
{
    [Header("Player Health")]
    [SerializeField] private bool respawnOnDeath = false;
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;

    [Header("References")]
    [SerializeField] private Player player;

    /// <summary>
    /// Modifies incoming damage based on equipped armor
    /// </summary>
    /// <param name="damage">Recived damage</param>
    protected override int ModifyIncomingDamage(int damage)
    {
        int totalProtection = 0;
        foreach (var armor in player.EquippedArmor.Values)
        {
            totalProtection += armor.protection;
        }

        return Mathf.Max(damage - totalProtection, 0);
    }

    /// <summary>
    /// Handles player death
    /// </summary>
    protected override void Die()
    {
        RaiseOnDeath();

        if (respawnOnDeath)
        {
            RespawnPlayer();
        }
        else
        {
            // For now just shows a message, can be expanded with game over screen
            Debug.Log("Player died! Game Over");
            // Here could show game over screen, restart level, etc.
        }
    }

    /// <summary>
    /// Respawns the player at the configured position
    /// </summary>
    private void RespawnPlayer()
    {
        transform.position = respawnPosition;
        ResetHealth();
        Debug.Log("Player respawned!");
    }

    /// <summary>
    /// Sets the respawn position
    /// </summary>
    /// <param name="position">New respawn position</param>
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }
}