using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image enemyHealthBarFill;
    [SerializeField] private TMP_Text enemyHealthText;
    [SerializeField] private Vector3 offset = new(0, 0, 0);
    
    private Transform target;   

    /// <summary>
    /// Sets up the enemy health bar if components are assigned
    /// </summary>
    public void Setup(EnemyHealth health, Transform targetTransform)
    {
        if (health != null && targetTransform != null)
        {
            target = targetTransform;
            health.OnHealthChanged += UpdateBar;
            UpdateBar(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.forward = Camera.main.transform.forward;
        }
    }

    private void UpdateBar(int current, int max)
    {
        if (enemyHealthBarFill != null)
        {
            enemyHealthBarFill.fillAmount = (float)current / max;
            enemyHealthText.text = $"{current}/{max}";
        }
    }
}