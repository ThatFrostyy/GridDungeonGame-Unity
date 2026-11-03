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
            transform.rotation = mainCamera.transform.rotation;
        }

        KeepUnmirrored();
    }

    /// <summary>
    /// Enemies flip their sprite with a negative x scale, which would mirror this child bar too.
    /// Cancel it out so the text always reads correctly.
    /// </summary>
    private void KeepUnmirrored()
    {
        Transform parent = transform.parent;
        float parentSign = parent != null && parent.lossyScale.x < 0f ? -1f : 1f;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * parentSign;
        transform.localScale = scale;
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
