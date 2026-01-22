using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Text _healthText;

    [Header("Visual Settings")]
    [SerializeField] private Color _fullHealthColor = Color.green;
    [SerializeField] private Color _mediumHealthColor = Color.yellow;
    [SerializeField] private Color _lowHealthColor = Color.red;
    [SerializeField] private float _lowHealthThreshold = 0.3f;
    [SerializeField] private float _mediumHealthThreshold = 0.6f;

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

        float healthPercentage = (float)currentHealth / (float)maxHealth;

        if (_animateChanges)
        {
            _targetFillAmount = healthPercentage;
        }
        else
        {
            _fillImage.fillAmount = healthPercentage;
        }

        UpdateColor(healthPercentage);

        if (_healthText != null)
        {
            _healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void UpdateColor(float healthPercentage)
    {
        Color targetColor;

        if (healthPercentage <= _lowHealthThreshold)
        {
            targetColor = _lowHealthColor;
        }
        else if (healthPercentage <= _mediumHealthThreshold)
        {
            targetColor = _mediumHealthColor;
        }
        else
        {
            targetColor = _fullHealthColor;
        }

        _fillImage.color = targetColor;
    }
}
