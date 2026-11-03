using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the player HUD (health bar and action points).
/// </summary>
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start (not Awake) so the locator and the player's health have been initialized.
    private void Start()
    {
        if (GameObjectLocator.Instance == null)
        {
            Debug.LogError("GameObjectLocator not found! Make sure it exists in the scene.", this);
            return;
        }

        playerHealth = GameObjectLocator.Instance.GetComponentByTag<PlayerHealth>("Player");
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }

        if (Instance == this)
        {
            Instance = null;
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
            healthBarFill.fillAmount = max > 0 ? (float)current / max : 0f;
        }

        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }
}
