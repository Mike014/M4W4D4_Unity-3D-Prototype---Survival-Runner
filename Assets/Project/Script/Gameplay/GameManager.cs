using UnityEngine;
using UnityEngine.UI; // Necessario per accedere al componente Text Legacy

/// <summary>
/// Gestisce lo stato globale del gioco: timer, raccolta monete, sblocco della porta.
/// Implementa il pattern Singleton per accesso globale da altri script.
/// Aggiorna la UI (timer e contatore monete) ogni frame.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    // Riferimento al componente Text che mostra il timer (MM:SS)
    [SerializeField] private Text _timerText;
    // Riferimento al componente Text che mostra il contatore monete (X / Y)
    [SerializeField] private Text _coinText;

    [Header("Timer Settings")]
    // Tempo disponibile all'inizio della partita (in secondi)
    [SerializeField] private float _timeRemaining = 120f;

    [Header("Level Settings")]
    // Riferimento alla porta che si sblocca quando il giocatore raccoglie abbastanza monete
    [SerializeField] private Door _exitDoor;
    // Numero totale di monete necessarie per vincere il livello
    public int RequiredCoins = 5;

    // ════════════════════════════════════════════════════════════════
    // VARIABILI INTERNE
    // ════════════════════════════════════════════════════════════════

    // Contatore delle monete raccolte finora
    private int _currentCoins = 0;
    // Flag che indica se la partita è terminata (per timeout)
    private bool _isGameOver = false;

    // SINGLETON PATTERN
    // Instance memorizza l'unica istanza del GameManager in tutta la scena
    // Tutti gli altri script accedono a questo tramite GameManager.Instance
    public static GameManager Instance;

    /// <summary>
    /// Implementazione del pattern Singleton.
    /// Assicura che esista una sola istanza di GameManager in tutta la scena.
    /// Se esiste già un'istanza, distrugge questa copia (duplicata).
    /// </summary>
    private void Awake()
    {
        // PATTERN SINGLETON
        // Awake() viene eseguito prima di Start(), durante l'inizializzazione della scena
        // È il momento giusto per implementare il Singleton

        if (Instance == null)
        {
            // Se Instance non esiste ancora, questa è la prima istanza
            // Salvala come istanza globale
            Instance = this;
        }
        else
        {
            // Se Instance già esiste, significa che c'è una copia duplicata
            // Distruggi questo GameObject duplicato per mantenere una sola istanza
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inizializza lo stato del gioco all'avvio della scena.
    /// Valida i riferimenti UI e aggiorna il display iniziale.
    /// </summary>
    void Start()
    {
        // RESET TIME SCALE
        // Time.timeScale = 1f assicura che il gioco corre a velocità normale
        // (utile se un'altra sezione del gioco ha messo in pausa il gioco)
        Time.timeScale = 1f;

        // INIZIALIZZAZIONE UI
        // Aggiorna il display delle monete con il valore iniziale (0 / RequiredCoins)
        UpdateCoinDisplay();

        // VALIDAZIONE RIFERIMENTI
        // Controlla che i componenti Text siano stati assegnati nell'Inspector
        // Se mancano, il gioco non funzionerà correttamente (niente display)
        if (_timerText == null || _coinText == null)
        {
            Debug.LogError("Assegna i componenti Text (Timer e Coins) nell'Inspector!");
        }
    }

    /// <summary>
    /// Aggiorna il gioco ogni frame.
    /// Decrementa il timer, aggiorna il display, e gestisce la fine della partita.
    /// </summary>
    void Update()
    {
        // Se la partita è già finita, non fare nulla
        // Questo previene aggiornamenti non necessari dopo GameOver
        if (_isGameOver) return;

        // LOGICA DEL TIMER
        if (_timeRemaining > 0)
        {
            // Il timer è ancora attivo, decrementa di Time.deltaTime ogni frame
            _timeRemaining -= Time.deltaTime;
            // Aggiorna il display del timer a schermo (MM:SS)
            UpdateTimerDisplay(_timeRemaining);
        }
        else
        {
            // Il timer ha raggiunto zero (tempo scaduto)
            _timeRemaining = 0; // Clamp a 0 per evitare numeri negativi
            UpdateTimerDisplay(0); // Mostra "00:00"
            GameOver(); // Termina la partita
        }
    }

    /// <summary>
    /// Aggiunge monete e tempo bonus al giocatore.
    /// Viene chiamato da Coin.cs quando il player raccoglie una moneta.
    /// Se la moneta è speciale, il tempo bonus viene raddoppiato.
    /// Se il numero di monete raggiunge il requisito, sblocca la porta di uscita.
    /// </summary>
    /// <param name="amount">Numero di monete da aggiungere</param>
    /// <param name="timeBonus">Secondi di tempo bonus da aggiungere al timer</param>
    /// <param name="isSpecial">Flag che indica se la moneta è speciale (raddoppia il bonus tempo)</param>
    public void AddCoin(int amount, float timeBonus, bool isSpecial = false)
    {
        // Incrementa il contatore delle monete raccolte
        _currentCoins += amount;

        // LOGICA DELLE MONETE SPECIALI
        // Se isSpecial è true, aggiungi il tempo bonus
        // Se isSpecial è false, NON aggiungere nessun tempo bonus
        if (isSpecial)
        {
            _timeRemaining += timeBonus;
            Debug.Log($"⭐ MONETA SPECIALE RACCOLTA! +{timeBonus} secondi bonus!");
        }
        else
        {
            Debug.Log($"Moneta normale raccolta. +0 secondi.");
        }

        // Aggiorna il display delle monete nel UI (es: "2 / 5")
        UpdateCoinDisplay();

        // CONDIZIONE DI VITTORIA
        // Se il giocatore ha raccolto abbastanza monete, sblocca la porta di uscita
        if (_currentCoins >= RequiredCoins)
        {
            UnlockDoor();
        }
    }

    /// <summary>
    /// Aggiorna il testo UI che mostra il contatore delle monete.
    /// Formato: "Monete: X / Y" (es: "Monete: 3 / 5")
    /// </summary>
    private void UpdateCoinDisplay()
    {
        // Valida che il riferimento al componente Text esista
        if (_coinText != null)
        {
            // Costruisce la stringa di testo con il numero attuale e il requisito
            // Usa concatenazione diretta per semplicità
            _coinText.text = "Monete: " + _currentCoins + " / " + RequiredCoins;
        }
    }

    /// <summary>
    /// Aggiorna il testo del timer nel UI e cambia colore se il tempo sta scadendo.
    /// Formato: MM:SS (es: "01:45" per 1 minuto e 45 secondi)
    /// Colore rosso quando rimangono 15 secondi o meno (avvertimento di urgenza).
    /// </summary>
    /// <param name="timeToDisplay">Tempo in secondi da mostrare nel display</param>
    void UpdateTimerDisplay(float timeToDisplay)
    {
        // CONVERSIONE TEMPO IN MINUTI E SECONDI
        // FloorToInt converte a intero troncando i decimali (non arrotonda)
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);  // Minuti interi
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);  // Secondi residui (operatore modulo %)

        // FORMATTAZIONE STRINGA
        // string.Format("{0:00}:{1:00}", minutes, seconds)
        // "{0:00}" = formatta il primo argomento (minutes) con 2 cifre (es: 01, 02, 10)
        // "{1:00}" = formatta il secondo argomento (seconds) con 2 cifre
        // Risultato: "01:30", "00:05", ecc.
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // CAMBIO COLORE PER AVVERTIMENTO
        // Se il tempo rimanente è 15 secondi o meno, cambia il colore del timer a rosso
        // Questo avvisa visivamente il giocatore che il tempo sta per scadere
        if (timeToDisplay <= 15f)
        {
            _timerText.color = Color.red;
        }
        else
        {
            // Reset al colore bianco quando il tempo è superiore a 15 secondi
            // (se era diventato rosso prima, torna alla normalità)
            _timerText.color = Color.white;
        }
    }

    /// <summary>
    /// Gestisce la fine della partita per timeout.
    /// Mette in pausa il gioco e mostra un messaggio di debug.
    /// </summary>
    void GameOver()
    {
        // Imposta il flag per indicare che la partita è terminata
        // Update() non farà più nulla una volta che _isGameOver è true
        _isGameOver = true;

        // PAUSA DEL GIOCO
        // Time.timeScale = 0f congela tutto (physics, deltaTime, animazioni)
        // Niente si muove o aggiorna finché timeScale non torna a 1
        // Utile per mostrare menu di pausa o schermata Game Over
        Time.timeScale = 0f;

        // DEBUG
        Debug.Log("Tempo Scaduto!");
    }

    /// <summary>
    /// Sblocca la porta di uscita quando il giocatore raccoglie abbastanza monete.
    /// Invoca il metodo Open() della porta assegnata.
    /// </summary>
    private void UnlockDoor()
    {
        // Valida che la porta sia stata assegnata nell'Inspector
        if (_exitDoor != null)
        {
            // Invoca il metodo Open() della porta per farla aprire
            _exitDoor.Open();
        }
    }
}