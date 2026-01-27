using UnityEngine;

/// <summary>
/// Rappresenta una moneta collezionabile nel gioco.
/// 
/// ARCHITETTURA EVENT-DRIVEN PURA (no Singleton):
/// - Trova GameEvents tramite FindObjectOfType
/// - Pubblica l'evento quando viene raccolta
/// - Non conosce GameManager
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _scoreValue = 1;
    [SerializeField] private float _timeBonus = 5f;
    [SerializeField] private bool _isSpecial = false;

    private GameEvents _gameEvents;

    // Trova GameEvents la prima volta che serve
    private GameEvents GetGameEvents()
    {
        if (_gameEvents == null)
        {
            _gameEvents = FindObjectOfType<GameEvents>();
            if (_gameEvents == null)
            {
                Debug.LogError("[COIN] GameEvents not found in scene!");
            }
        }
        return _gameEvents;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[COIN DEBUG] _isSpecial value = {_isSpecial}");
            Debug.Log($"[COIN DEBUG] _scoreValue = {_scoreValue}");
            Debug.Log($"[COIN DEBUG] _timeBonus = {_timeBonus}");
            
            // ✅ Trova GameEvents e pubblica l'evento
            GameEvents gameEvents = GetGameEvents();
            if (gameEvents != null)
            {
                gameEvents.PublishCoinCollected(_scoreValue, _timeBonus, _isSpecial);
            }

            Destroy(gameObject);
        }
    }
}