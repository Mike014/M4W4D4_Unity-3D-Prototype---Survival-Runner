using System;
using UnityEngine;

/// <summary>
/// Hub centrale degli eventi del gioco - MonoBehaviour SENZA Singleton.
/// Non ha istanza statica, viene trovato tramite FindObjectOfType.
/// 
/// VANTAGGI:
/// ✓ Non è un Singleton - meno accoppiamento
/// ✓ Può essere distrutto e ricreato senza problemi
/// ✓ Testabile: puoi creare istanze fake nei test
/// ✓ Puoi avere più istanze se necessario (anche se non lo consigli)
/// 
/// COME USARE:
/// - Gli script trovano questa istanza automaticamente tramite FindObjectOfType
/// - Se non esiste, ricevono un warning ma il gioco continua
/// 
/// SETUP:
/// 1. Crea un GameObject vuoto chiamato "_EventManager" nella scena
/// 2. Aggiungi questo script al GameObject
/// 3. Non serve assegnare riferimenti: script trovano automaticamente
/// </summary>
public class GameEvents : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════
    // EVENTI PUBBLICI - Chiunque può sottoscrivere
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pubblicato quando il player raccoglie una moneta.
    /// I parametri sono: (amount, timeBonus, isSpecial)
    /// </summary>
    public event Action<int, float, bool> OnCoinCollected;

    /// <summary>
    /// Pubblicato quando la partita termina (vittoria o sconfitta).
    /// Il parametro è: (hasWon)
    /// </summary>
    public event Action<bool> OnGameOver;

    /// <summary>
    /// Pubblicato ogni volta che il timer cambia.
    /// Il parametro è: (timeRemaining)
    /// </summary>
    public event Action<float> OnTimeChanged;

    /// <summary>
    /// Pubblicato quando il contatore di monete cambia.
    /// Il parametro è: (currentCoins, requiredCoins)
    /// </summary>
    public event Action<int, int> OnCoinCountChanged;

    /// <summary>
    /// Pubblicato quando il player ha abbastanza monete per vincere.
    /// </summary>
    public event Action OnVictoryConditionMet;


    // ════════════════════════════════════════════════════════════════
    // METODI PUBBLICI - Per pubblicare gli eventi
    // ════════════════════════════════════════════════════════════════

    public void PublishCoinCollected(int amount, float timeBonus, bool isSpecial)
    {
        Debug.Log($"[GameEvents] Coin Collected: +{amount} points, +{timeBonus}s bonus, Special={isSpecial}");
        OnCoinCollected?.Invoke(amount, timeBonus, isSpecial);
    }

    public void PublishGameOver(bool hasWon)
    {
        Debug.Log($"[GameEvents] Game Over - Won: {hasWon}");
        OnGameOver?.Invoke(hasWon);
    }

    public void PublishTimeChanged(float timeRemaining)
    {
        OnTimeChanged?.Invoke(timeRemaining);
    }

    public void PublishCoinCountChanged(int currentCoins, int requiredCoins)
    {
        Debug.Log($"[GameEvents] Coin Count Changed: {currentCoins}/{requiredCoins}");
        OnCoinCountChanged?.Invoke(currentCoins, requiredCoins);
    }

    public void PublishVictoryConditionMet()
    {
        Debug.Log("[GameEvents] Victory Condition Met!");
        OnVictoryConditionMet?.Invoke();
    }
}