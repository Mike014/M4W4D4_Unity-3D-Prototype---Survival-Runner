using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Text _healthText;

    [Header("Animation Settings")]
    [SerializeField] private bool _animateChanges = true;
    [SerializeField] private float _animationSpeed = 5f;

    private float _targetFillAmount = 1f;

    void Start()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
        else
        {
            Debug.LogError("PlayerHealth reference missing on HealthBar!");
        }
    }

    void OnDestroy()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }

    void Update()
    {
        // Gestione animazione fluida
        if (_animateChanges && _fillImage != null)
        {
            _fillImage.fillAmount = Mathf.Lerp(
                _fillImage.fillAmount, 
                _targetFillAmount, 
                _animationSpeed * Time.deltaTime
            );
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (_fillImage == null) return;

        // Calcolo percentuale (0.0 - 1.0)
        float healthPercentage = (float)currentHealth / (float)maxHealth;

        if (_animateChanges)
        {
            _targetFillAmount = healthPercentage;
        }
        else
        {
            _fillImage.fillAmount = healthPercentage;
        }

        // Aggiornamento testo opzionale
        if (_healthText != null)
        {
            _healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
}