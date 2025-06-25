using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text apText;

    private PlayerHealth playerHealth;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (GameObjectLocator.Instance != null)
        {
            playerHealth = GameObjectLocator.Instance.GetComponentByTag<PlayerHealth>("Player");

        }
        else
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
        }

        SetupPlayerHealthBar();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    /// <summary>
    /// Sets up the player health bar if components are assigned
    /// </summary>
    private void SetupPlayerHealthBar()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    public void UpdateAP(int current, int max)
    {
        if (apText != null)
        {
            apText.text = $"AP: {current}/{max}";
        }
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)current / max;
            healthText.text = $"{current}/{max}";
        }
    }
}