using UnityEngine;
using UnityEngine.UI; // Necessario per accedere ai componenti UI
using UnityEngine.SceneManagement; // Necessario per gestire le scene

/// <summary>
/// Gestisce lo stato globale del gioco: timer, raccolta monete, sblocco della porta.
/// Implementa il pattern Singleton per accesso globale da altri script.
/// Aggiorna la UI (timer e contatore monete) ogni frame.
/// Gestisce le schermate di vittoria e sconfitta con button per il menu e restart.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    // Riferimento al componente Text che mostra il timer (MM:SS)
    [SerializeField] private Text _timerText;
    // Riferimento al componente Text che mostra il contatore monete (X / Y)
    [SerializeField] private Text _coinText;

    [Header("End Game UI")]
    // Immagine che appare quando il giocatore VINCE (contiene button e testo)
    [SerializeField] private Image _victoryImage;
    // Bottone nel pannello di vittoria per tornare al menu
    [SerializeField] private Button _backToMenuButtonVictory;

    // Immagine che appare quando il giocatore PERDE (contiene button e testo)
    [SerializeField] private Image _defeatImage;
    // Bottone nel pannello di sconfitta per ricominciare la partita
    [SerializeField] private Button _restartButtonDefeat;
    // Bottone nel pannello di sconfitta per tornare al menu
    [SerializeField] private Button _backToMenuButtonDefeat;

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
    // Flag che indica se la partita è terminata (per timeout o vittoria)
    private bool _isGameOver = false;
    // Flag che indica se il giocatore ha VINTO (true) o PERSO (false)
    private bool _hasWon = false;

    // SINGLETON PATTERN
    // Instance memorizza l'unica istanza del GameManager in tutta la scena
    // Tutti gli altri script accedono a questo tramite GameManager.Instance
    public static GameManager Instance;

    /// <summary>
    /// Implementazione del pattern Singleton.
    /// Assicura che esista una sola istanza di GameManager in tutta la scena.
    /// Se esiste già un'istanza, distrugge questa copia (duplicata).
    /// Awake() viene eseguito prima di Start(), durante l'inizializzazione della scena.
    /// È il momento giusto per implementare il Singleton.
    /// </summary>
    private void Awake()
    {
        // PATTERN SINGLETON
        // Verifica se esiste già un'istanza del GameManager
        if (Instance == null)
        {
            // Se Instance non esiste ancora, questa è la prima istanza
            // Salvala come istanza globale per l'accesso da altri script
            Instance = this;
        }
        else
        {
            // Se Instance già esiste, significa che c'è una copia duplicata
            // (probabilmente perché la scena è stata ricaricata)
            // Distruggi questo GameObject duplicato per mantenere una sola istanza
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inizializza lo stato del gioco all'avvio della scena.
    /// Valida i riferimenti UI, aggiorna il display iniziale,
    /// nasconde i pannelli di vittoria/sconfitta,
    /// e collega i bottoni ai rispettivi metodi.
    /// </summary>
    void Start()
    {
        // RESET TIME SCALE
        // Time.timeScale = 1f assicura che il gioco corre a velocità normale
        // (utile se un'altra sezione del gioco aveva messo in pausa il gioco)
        Time.timeScale = 1f;

        // SETUP DELLE IMMAGINI DI END GAME
        if (_victoryImage != null)
        {
            _victoryImage.enabled = false;
            Debug.Log("Victory Image trovata e disabilitata");
        }
        else
        {
            Debug.LogError("Victory Image NON assegnata!");
        }

        if (_defeatImage != null)
        {
            _defeatImage.enabled = false;
            Debug.Log("Defeat Image trovata e disabilitata");
        }
        else
        {
            Debug.LogError("Defeat Image NON assegnata!");
        }

        // BLOCCA IL CURSORE ALL'INIZIO DEL GIOCO
        // Il cursore viene bloccato al centro dello schermo per la camera di gioco
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // INIZIALIZZAZIONE UI
        // Aggiorna il display delle monete con il valore iniziale (0 / RequiredCoins)
        UpdateCoinDisplay();

        // VALIDAZIONE RIFERIMENTI UI
        // Controlla che i componenti Text siano stati assegnati nell'Inspector
        // Se mancano, il gioco non funzionerà correttamente (niente display)
        if (_timerText == null || _coinText == null)
        {
            Debug.LogError("Assegna i componenti Text (Timer e Coins) nell'Inspector!");
        }

        // SETUP DELLE IMMAGINI DI END GAME
        // Assicurati che le immagini di vittoria e sconfitta siano invisibili all'inizio
        if (_victoryImage != null)
        {
            _victoryImage.enabled = false;
        }
        else
        {
            Debug.LogError("Victory Image non assegnata nell'Inspector!");
        }

        if (_defeatImage != null)
        {
            _defeatImage.enabled = false;
        }
        else
        {
            Debug.LogError("Defeat Image non assegnata nell'Inspector!");
        }

        // COLLEGAMENTO DEI BOTTONI DI VITTORIA
        if (_backToMenuButtonVictory != null)
        {
            // Registra il metodo BackToMenu() come listener del click event
            _backToMenuButtonVictory.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Back To Menu Button (Victory) non assegnato nell'Inspector!");
        }

        // COLLEGAMENTO DEI BOTTONI DI SCONFITTA
        if (_restartButtonDefeat != null)
        {
            // Registra il metodo RestartGame() come listener del click event
            _restartButtonDefeat.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("Restart Button (Defeat) non assegnato nell'Inspector!");
        }

        if (_backToMenuButtonDefeat != null)
        {
            // Registra il metodo BackToMenu() come listener del click event
            _backToMenuButtonDefeat.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Back To Menu Button (Defeat) non assegnato nell'Inspector!");
        }
    }

    /// <summary>
    /// Aggiorna il gioco ogni frame.
    /// Decrementa il timer, aggiorna il display, e gestisce la fine della partita per timeout.
    /// Se il timer scade, il giocatore PERDE.
    /// </summary>
    void Update()
    {
        // PROTEZIONE: Se la partita è già finita, non eseguire più logica di gioco
        // Questo previene aggiornamenti non necessari e comportamenti strani dopo GameOver
        if (_isGameOver) return;

        // LOGICA DEL TIMER
        // Il timer scende costantemente durante il gioco
        if (_timeRemaining > 0)
        {
            // Il timer è ancora attivo, decrementa di Time.deltaTime ogni frame
            // Time.deltaTime = tempo trascorso dall'ultimo frame (framerate-independent)
            _timeRemaining -= Time.deltaTime;
            // Aggiorna il display visuale del timer a schermo (formato MM:SS)
            UpdateTimerDisplay(_timeRemaining);
        }
        else
        {
            // Il timer ha raggiunto zero (tempo scaduto)
            // Clamp il valore a 0 per evitare numeri negativi
            _timeRemaining = 0;
            // Mostra "00:00" nel display
            UpdateTimerDisplay(0);
            // Termina la partita (il giocatore HA PERSO perché il tempo è scaduto)
            // false = ha perso
            GameOver(false);
        }
    }

    /// <summary>
    /// Aggiunge monete e tempo bonus al giocatore.
    /// Viene chiamato da Coin.cs quando il player raccoglie una moneta.
    /// Se la moneta è speciale (isSpecial = true), il tempo bonus viene aggiunto.
    /// Se il numero di monete raccolte raggiunge il requisito, il giocatore VINCE.
    /// </summary>
    /// <param name="amount">Numero di monete da aggiungere al contatore</param>
    /// <param name="timeBonus">Secondi di tempo bonus da aggiungere al timer (SOLO se isSpecial = true)</param>
    /// <param name="isSpecial">Flag che indica se la moneta è speciale (aggiunge tempo bonus)</param>
    public void AddCoin(int amount, float timeBonus, bool isSpecial = false)
    {
        // PROTEZIONE: Se la partita è già finita, non raccogliere più monete
        // Questo previene anomalie se il player raccoglie monete durante la schermata di fine gioco
        if (_isGameOver)
        {
            return;
        }

        // INCREMENTO DELLE MONETE
        // Aggiunge il numero di monete specificate al contatore totale
        _currentCoins += amount;

        // LOGICA DELLE MONETE SPECIALI
        // Se isSpecial è true, aggiungi il tempo bonus al timer
        // Se isSpecial è false, NON aggiungere nessun tempo bonus (solo conteggio monete)
        if (isSpecial)
        {
            // Moneta speciale: aggiungi il tempo bonus
            _timeRemaining += timeBonus;
            Debug.Log($"⭐ MONETA SPECIALE RACCOLTA! +{timeBonus} secondi bonus!");
        }
        else
        {
            // Moneta normale: solo conteggio, nessun bonus tempo
            Debug.Log($"Moneta normale raccolta. +0 secondi.");
        }

        // AGGIORNAMENTO DEL DISPLAY
        // Aggiorna il testo UI che mostra il contatore (es: "2 / 5")
        UpdateCoinDisplay();

        // CONDIZIONE DI VITTORIA
        // Se il giocatore ha raccolto abbastanza monete, HA VINTO il livello!
        if (_currentCoins >= RequiredCoins)
        {
            // Termina la partita (il giocatore HA VINTO)
            // true = ha vinto
            GameOver(true);
        }
    }

    /// <summary>
    /// Aggiorna il testo UI che mostra il contatore delle monete.
    /// Formato: "Monete: X / Y" (es: "Monete: 3 / 5")
    /// Viene chiamato ogni volta che il giocatore raccoglie una moneta.
    /// </summary>
    private void UpdateCoinDisplay()
    {
        // VALIDAZIONE: Controlla che il riferimento al componente Text esista
        if (_coinText != null)
        {
            // Costruisce la stringa di testo con il numero attuale e il requisito
            // Usa concatenazione con l'operatore + per semplicità
            // Esempio: "Monete: " + 2 + " / " + 5 = "Monete: 2 / 5"
            _coinText.text = "Monete: " + _currentCoins + " / " + RequiredCoins;
        }
    }

    /// <summary>
    /// Aggiorna il testo del timer nel UI e cambia colore se il tempo sta scadendo.
    /// Formato: MM:SS (es: "01:45" per 1 minuto e 45 secondi)
    /// Colore rosso quando rimangono 15 secondi o meno (avvertimento di urgenza visivo).
    /// Viene chiamato ogni frame dall'Update().
    /// </summary>
    /// <param name="timeToDisplay">Tempo in secondi da mostrare nel display</param>
    void UpdateTimerDisplay(float timeToDisplay)
    {
        // CONVERSIONE TEMPO IN MINUTI E SECONDI
        // Mathf.FloorToInt converte a intero troncando i decimali (non arrotonda)
        // Es: 95.7 secondi → 1 minuto e 35 secondi

        // Minuti: dividi i secondi totali per 60, prendi la parte intera
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);

        // Secondi: usa l'operatore modulo (%) per ottenere il resto
        // Es: 95.7 % 60 = 35.7, FloorToInt(35.7) = 35
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // FORMATTAZIONE STRINGA
        // string.Format("{0:00}:{1:00}", minutes, seconds)
        // "{0:00}" = formatta il primo argomento (minutes) con 2 cifre minimo (es: 01, 02, 10)
        // Se è un numero a una cifra, aggiunge uno 0 davanti (padding)
        // "{1:00}" = formatta il secondo argomento (seconds) con lo stesso sistema
        // Risultato: "01:30", "00:05", "02:45", ecc.
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // CAMBIO COLORE PER AVVERTIMENTO URGENZA
        // Se il tempo rimanente è 15 secondi o meno, cambia il colore a rosso
        // Questo avvisa visivamente il giocatore che il tempo sta per scadere
        if (timeToDisplay <= 15f)
        {
            _timerText.color = Color.red;
        }
        else
        {
            // Se il tempo è superiore a 15 secondi, reset al colore bianco
            // (se era diventato rosso prima, torna alla normalità quando il giocatore raccoglie tempo bonus)
            _timerText.color = Color.white;
        }
    }

    /// <summary>
    /// Gestisce la fine della partita (vittoria o sconfitta).
    /// Mette in pausa il gioco (Time.timeScale = 0), mostra il cursore,
    /// e visualizza l'immagine appropriata (vittoria o sconfitta).
    /// </summary>
    /// <param name="hasWon">True se il giocatore ha VINTO, False se ha PERSO</param>
    public void GameOver(bool hasWon)
    {
        if (_isGameOver)
        {
            return;
        }

        _isGameOver = true;
        _hasWon = hasWon;

        Debug.Log($"[GameManager] GameOver chiamato - HasWon: {hasWon}");

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (hasWon)
        {
            if (_victoryImage != null)
            {
                // ✅ Abilita il GAMEOBJECT (non solo l'Image)
                _victoryImage.gameObject.SetActive(true);

                Debug.Log("🎉 HAI VINTO! Immagine di vittoria visualizzata!");
            }
            else
            {
                Debug.LogError("Victory Image non trovata!");
            }
        }
        else
        {
            if (_defeatImage != null)
            {
                // ✅ Abilita il GAMEOBJECT (non solo l'Image)
                _defeatImage.gameObject.SetActive(true);

                Debug.Log("💀 HAI PERSO! Immagine di sconfitta visualizzata!");
            }
            else
            {
                Debug.LogError("Defeat Image non trovata!");
            }
        }
    }

    /// <summary>
    /// Torna al menu principale.
    /// Viene chiamato dai bottoni "Back To Menu" nelle immagini di vittoria e sconfitta.
    /// Ripristina il tempo normale e carica la scena del menu.
    /// Assicurati che il menu sia nella Build Settings (File > Build Settings) con indice 0.
    /// </summary>
    private void BackToMenu()
    {
        // RESET TIME SCALE
        // Ripristina il tempo a velocità normale prima di cambiare scena
        // (importante perché Time.timeScale = 0 era stato impostato in GameOver)
        Time.timeScale = 1f;

        // MOSTRA IL CURSORE
        // Il cursore rimane visibile nel menu (non c'è bisogno di bloccarlo)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // CARICAMENTO DELLA SCENA DEL MENU
        // Carica la scena con indice 0 (il menu principale)
        // SceneManager.LoadScene() scarica la scena attuale e carica quella nuova
        Debug.Log("Ritorno al menu principale...");
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Ricomincia la partita attuale.
    /// Viene chiamato dal bottone "Restart" nell'immagine di sconfitta.
    /// Ripristina il tempo normale e ricarica la scena per ricominciare da zero.
    /// </summary>
    private void RestartGame()
    {
        // RESET TIME SCALE
        // Ripristina il tempo a velocità normale prima di ricaricare
        Time.timeScale = 1f;

        // BLOCCA IL CURSORE
        // Quando ricomincia il gioco, il cursore deve essere bloccato al centro
        // (per la camera orbit di gioco)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // CARICAMENTO DELLA SCENA
        // SceneManager.GetActiveScene().buildIndex ritorna l'indice della scena attuale
        // Ricaricando la scena con lo stesso indice, resettiamo tutto il gioco
        Debug.Log("Riavvio della partita...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Sblocca la porta di uscita quando il giocatore raccoglie abbastanza monete.
    /// Invoca il metodo Open() della porta assegnata.
    /// Questo metodo non è più essenziale con il nuovo sistema di vittoria,
    /// ma rimane per compatibilità con script esterni.
    /// </summary>
    private void UnlockDoor()
    {
        // VALIDAZIONE: Controlla che la porta sia stata assegnata nell'Inspector
        if (_exitDoor != null)
        {
            // Invoca il metodo Open() della porta per farla aprire
            _exitDoor.Open();
        }
    }

    /// <summary>
    /// Pulizia: rimuove i listener dai bottoni quando il GameManager viene distrutto.
    /// È importante disiscriversi dagli eventi per evitare memory leaks
    /// (riferimenti orfani a script distrutti).
    /// </summary>
    void OnDestroy()
    {
        // UNSUBSCRIBE DAI BOTTONI DI VITTORIA
        // Rimuove questo script dalla lista di listener del click event
        if (_backToMenuButtonVictory != null)
        {
            _backToMenuButtonVictory.onClick.RemoveListener(BackToMenu);
        }

        // UNSUBSCRIBE DAI BOTTONI DI SCONFITTA
        // Rimuove il listener dal bottone "Restart"
        if (_restartButtonDefeat != null)
        {
            _restartButtonDefeat.onClick.RemoveListener(RestartGame);
        }
        // Rimuove il listener dal bottone "Back To Menu"
        if (_backToMenuButtonDefeat != null)
        {
            _backToMenuButtonDefeat.onClick.RemoveListener(BackToMenu);
        }
    }
}