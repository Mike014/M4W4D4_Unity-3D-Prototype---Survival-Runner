using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [Header("Invincibility Settings")]
    [SerializeField] private bool _enableInvincibilityFrames = true;
    [SerializeField] private float _invincibilityDuration = 1f;

    private bool _isInvincible = false;
    private float _invincibilityTimer = 0f;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnHealed;

    // Properties
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;
    public bool IsInvincible => _isInvincible;

    void Start()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    void Update()
    {
        if (_isInvincible)
        {
            _invincibilityTimer -= Time.deltaTime;
            if (_invincibilityTimer <= 0f)
            {
                _isInvincible = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isInvincible || IsDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        OnDamageTaken?.Invoke();

        if (_enableInvincibilityFrames && _currentHealth > 0)
        {
            _isInvincible = true;
            _invincibilityTimer = _invincibilityDuration;
        }

        Debug.Log($"Damage Taken: {damage}. Health: {_currentHealth}/{_maxHealth}");

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (IsDead) return;

        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        OnHealed?.Invoke();

        Debug.Log($"Healed: {healAmount}. Health: {_currentHealth}/{_maxHealth}");
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        _maxHealth = newMaxHealth;

        if (healToFull)
        {
            _currentHealth = _maxHealth;
        }
        else
        {
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player Died");
        OnDeath?.Invoke();

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    // Debug UI
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 130, 300, 30), $"Health: {_currentHealth}/{_maxHealth}", style);
        GUI.Label(new Rect(10, 160, 300, 30), $"Invincible: {_isInvincible}", style);

        if (GUI.Button(new Rect(10, 190, 150, 30), "Take 10 Damage"))
        {
            TakeDamage(10);
        }

        if (GUI.Button(new Rect(10, 230, 150, 30), "Heal 20"))
        {
            Heal(20);
        }
    }
}
