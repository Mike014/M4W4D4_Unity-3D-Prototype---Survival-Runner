using UnityEngine;

/// <summary>
/// Rappresenta una moneta collezionabile nel gioco.
/// Quando il player la raccoglie, aggiunge punti e tempo bonus al GameManager.
/// La moneta può essere normale o speciale (parametri modificabili in Inspector).
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Settings")]
    // Punti aggiunto al punteggio totale quando il player raccoglie la moneta
    [SerializeField] private int _scoreValue = 1;
    // Tempo in secondi aggiunto al timer del gioco quando raccolta
    // (Nota: assicurati che il GameManager accetti questo valore come float, non int)
    [SerializeField] private float _timeBonus = 5f;
    // Flag che indica se questa è una moneta speciale (es: doppi punti, effetti diversi)
    // Utile per comportamenti condizionali futuri
    // IMPORTANTE: Imposta questo SOLO da Inspector, non viene modificato nel codice
    [SerializeField] private bool _isSpecial = false;

    /// <summary>
    /// Gestisce il contatto del player con la moneta.
    /// Se il player la raccoglie, notifica il GameManager con il flag isSpecial e distrugge la moneta.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // VALIDAZIONE: Controlla che l'oggetto che entra sia il player
        if (other.CompareTag("Player"))
        {
            // DEBUG PRECISO
            Debug.Log($"[COIN DEBUG] _isSpecial value = {_isSpecial}");
            Debug.Log($"[COIN DEBUG] _scoreValue = {_scoreValue}");
            Debug.Log($"[COIN DEBUG] _timeBonus = {_timeBonus}");
            
            // ACCESSO AL SINGLETON
            // GameManager.Instance è un pattern Singleton che fornisce accesso globale
            // al gestore del gioco (gestisce punti, tempo, stato generale)
            // Verifica che Instance non sia null (protezione da crash se GameManager non esiste)
            if (GameManager.Instance != null)
            {
                // NOTIFICA AL GAMEMANAGER
                // Comunica al GameManager che il player ha raccolto una moneta
                // Passa:
                //   - _scoreValue: punti da aggiungere
                //   - _timeBonus: secondi da aggiungere al timer
                //   - _isSpecial: flag che indica se la moneta è speciale (doppio tempo bonus)
                GameManager.Instance.AddCoin(_scoreValue, _timeBonus, _isSpecial);
            }

            // DISTRUZIONE DELLA MONETA
            // Rimuove la moneta dalla scena (è stata raccolta)
            // Importante: Destroy() rimuove il GameObject completamente,
            // liberando la memoria allocata per questo oggetto
            Destroy(gameObject);
        }
    }
}