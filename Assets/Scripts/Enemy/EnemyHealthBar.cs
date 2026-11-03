using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar that follows an enemy and faces the camera.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image enemyHealthBarFill;
    [SerializeField] private TMP_Text enemyHealthText;
    [SerializeField] private Vector3 offset = new(0, 0, 0);

    private Transform target;
    private EnemyHealth health;
    private Camera mainCamera;

    public void Setup(EnemyHealth enemyHealth, Transform targetTransform)
    {
        if (enemyHealth == null || targetTransform == null)
        {
            return;
        }

        if (health != null)
        {
            health.OnHealthChanged -= UpdateBar;
        }

        health = enemyHealth;
        target = targetTransform;
        health.OnHealthChanged += UpdateBar;
        UpdateBar(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateBar;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }

    private void UpdateBar(int current, int max)
    {
        if (enemyHealthBarFill != null)
        {
            enemyHealthBarFill.fillAmount = max > 0 ? (float)current / max : 0f;
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"{current}/{max}";
        }
    }
}
