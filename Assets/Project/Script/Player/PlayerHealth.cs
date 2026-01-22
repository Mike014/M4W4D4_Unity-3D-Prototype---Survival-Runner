using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestisce il sistema di salute del player.
/// Traccia la vita attuale, notifica gli eventi quando il player subisce danno/guarigione/morte,
/// e disabilita il controllo del player quando muore.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    // La salute massima del player - modificabile in Inspector
    [SerializeField] private int _maxHealth = 100;
    // La salute attuale del player - inizializzata in Start()
    [SerializeField] private int _currentHealth;

    [Header("Events")]
    // Evento invocato quando la salute cambia: passa (salute_attuale, salute_massima)
    public UnityEvent<int, int> OnHealthChanged;
    // Evento invocato quando il player muore
    public UnityEvent OnDeath;
    // Evento invocato quando il player subisce danno
    public UnityEvent OnDamageTaken;
    // Evento invocato quando il player si guarisce
    public UnityEvent OnHealed;

    // Properties - permettono di leggere i valori ma non modificarli direttamente dall'esterno
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;

    void Start()
    {
        // Inizializza la salute attuale al massimo valore
        _currentHealth = _maxHealth;
        // Notifica i listener che la salute è stata inizializzata
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    /// <summary>
    /// Riduce la salute del player di una certa quantità.
    /// </summary>
    /// <param name="damage">Quantità di danno da infliggere</param>
    public void TakeDamage(int damage)
    {
        // Se siamo già morti, ignoriamo il danno (prevenire danno postume)
        if (IsDead) return;

        // Sottrai il danno dalla salute attuale
        _currentHealth -= damage;
        // Clamp assicura che la salute rimanga tra 0 e _maxHealth
        // (non può scendere sotto 0, non può salire oltre il massimo)
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        // Notifica tutti i listener che la salute è cambiata
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        // Notifica che il danno è stato subito
        OnDamageTaken?.Invoke();

        // Log per debug durante lo sviluppo
        Debug.Log($"Damage Taken: {damage}. Health: {_currentHealth}/{_maxHealth}");

        // Se la salute raggiunge 0 o meno, il player muore
        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Aumenta la salute del player di una certa quantità.
    /// Non funziona se il player è morto.
    /// </summary>
    /// <param name="healAmount">Quantità di guarigione</param>
    public void Heal(int healAmount)
    {
        // Se siamo già morti, non possiamo guarirci (la morte è finale)
        if (IsDead) return;

        // Aggiungi la guarigione alla salute attuale
        _currentHealth += healAmount;
        // Clamp assicura che la salute non superi il massimo e resti sopra lo 0
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        // Notifica i listener che la salute è cambiata
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        // Notifica che il player è stato guarito
        OnHealed?.Invoke();

        // Log per debug durante lo sviluppo
        Debug.Log($"Healed: {healAmount}. Health: {_currentHealth}/{_maxHealth}");
    }

    /// <summary>
    /// Modifica la salute massima del player.
    /// </summary>
    /// <param name="newMaxHealth">Il nuovo valore massimo di salute</param>
    /// <param name="healToFull">Se true, riempie la salute fino al nuovo massimo. 
    ///                          Se false, mantiene la salute attuale (ma clampata al nuovo massimo)</param>
    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        // Aggiorna il valore massimo di salute
        _maxHealth = newMaxHealth;

        if (healToFull)
        {
            // Opzione 1: Riempi completamente la salute al nuovo massimo
            _currentHealth = _maxHealth;
        }
        else
        {
            // Opzione 2: Mantieni la salute attuale, ma assicurati che non ecceda il nuovo massimo
            // (Es: se avevi 80 salute su 100, e abbasso il massimo a 50, diventi 50)
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }

        // Notifica i listener che la salute è cambiata (sia il valore attuale che il massimo)
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    /// <summary>
    /// Gestisce la morte del player.
    /// Disabilita i controlli e invoca l'evento OnDeath.
    /// </summary>
    private void Die()
    {
        // Log per debug
        Debug.Log("Player Died");
        // Notifica tutti i listener che il player è morto
        OnDeath?.Invoke();

        // ← NUOVO: Avvisa il GameManager che il player è morto
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false); // false = ha perso (morto)
        }

        // Cerca il componente PlayerController sullo stesso GameObject
        PlayerController controller = GetComponent<PlayerController>();
        // Se esiste un PlayerController, disabilitalo per impedire ulteriori movimenti/azioni
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
}