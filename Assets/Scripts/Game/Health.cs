using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base class for handling character health (player and enemies)
/// Provides common functionality like taking damage, healing, and visual effects
/// </summary>
public abstract class Health : MonoBehaviour
{
    [Header("Health Configuration")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;

    [Header("Visual Effects")]
    [SerializeField] protected GameObject hitEffectPrefab;
    [SerializeField] protected float flickerDuration = 0.3f;
    [SerializeField] protected int flickerCount = 3;

    protected List<SpriteRenderer> spriteRenderers;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    /// <summary>
    /// Event triggered when health changes (current health, max health)
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// Event triggered when the character dies
    /// </summary>
    public event Action OnDeath;

    protected virtual void Awake()
    {
        InitializeHealth();
    }

    /// <summary>
    /// Initializes health and caches SpriteRenderer components
    /// </summary>
    protected virtual void InitializeHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        CacheSpriteRenderers();
    }

    /// <summary>
    /// Caches SpriteRenderer components for visual effects
    /// </summary>
    protected virtual void CacheSpriteRenderers()
    {
        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));
    }

    /// <summary>
    /// Applies damage to the character
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        SpawnHitEffect();
        PlayFlickerEffect();

        if (currentHealth == 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the character
    /// </summary>
    /// <param name="amount">Amount of health to restore</param>
    public virtual void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Resets health to maximum
    /// </summary>
    public virtual void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Checks if the character is alive
    /// </summary>
    public bool IsAlive => currentHealth > 0;

    /// <summary>
    /// Gets the current health percentage (0.0 - 1.0)
    /// </summary>
    public float HealthPercentage => (float)currentHealth / maxHealth;

    /// <summary>
    /// Spawns the hit effect visual
    /// </summary>
    protected virtual void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Starts the flicker visual effect
    /// </summary>
    protected virtual void PlayFlickerEffect()
    {
        if (spriteRenderers != null && spriteRenderers.Count > 0)
        {
            StartCoroutine(FlickerCoroutine());
        }
    }

    /// <summary>
    /// Coroutine that handles the flicker effect
    /// </summary>
    protected virtual IEnumerator FlickerCoroutine()
    {
        float flickerInterval = flickerDuration / (flickerCount * 2f);

        for (int i = 0; i < flickerCount; i++)
        {
            SetRenderersEnabled(false);
            yield return new WaitForSeconds(flickerInterval);
            SetRenderersEnabled(true);
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    /// <summary>
    /// Enables or disables the SpriteRenderers
    /// </summary>
    protected virtual void SetRenderersEnabled(bool enabled)
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = enabled;
            }
        }
    }

    /// <summary>
    /// Handles character death - must be implemented by derived classes
    /// </summary>
    protected abstract void Die();
} 