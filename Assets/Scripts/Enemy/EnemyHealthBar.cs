using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Vector3 offset = new(0, 0, 0);

    private Transform target;

    public void Setup(EnemyHealth health, Transform targetTransform)
    {
        target = targetTransform;
        health.OnHealthChanged += UpdateBar;
        UpdateBar(health.CurrentHealth, health.MaxHealth);
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
        if (fillImage != null)
        {
            fillImage.fillAmount = (float)current / max;
        }
        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }
}