using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float flickerDuration = 0.3f;
    [SerializeField] private int flickerCount = 3;

    private List<SpriteRenderer> spriteRenderers;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        if (spriteRenderers != null && spriteRenderers.Count > 0)
        {
            StartCoroutine(Flicker());
        }

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private IEnumerator Flicker()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            SetRenderersEnabled(false);
            yield return new WaitForSeconds(flickerDuration / (flickerCount * 2f));
            SetRenderersEnabled(true);
            yield return new WaitForSeconds(flickerDuration / (flickerCount * 2f));
        }
    }

    private void SetRenderersEnabled(bool enabled)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
                sr.enabled = enabled;
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}