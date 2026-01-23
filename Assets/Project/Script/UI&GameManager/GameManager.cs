using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestisce lo stato globale del gioco: timer, raccolta monete, sblocco della porta.
/// Implementa il pattern Singleton per accesso globale da altri script.
/// 
/// REFACTORING: La logica di gioco è separata dalle dipendenze UI/Scene
/// per consentire testing senza mockare componenti Unity complessi.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _coinText;

    [Header("End Game UI")]
    [SerializeField] private Image _victoryImage;
    [SerializeField] private Button _backToMenuButtonVictory;
    [SerializeField] private Image _defeatImage;
    [SerializeField] private Button _restartButtonDefeat;
    [SerializeField] private Button _backToMenuButtonDefeat;

    [Header("Timer Settings")]
    [SerializeField] private float _timeRemaining = 120f;

    [Header("Level Settings")]
    [SerializeField] private Door _exitDoor;
    public int RequiredCoins = 5;

    // ════════════════════════════════════════════════════════════════
    // VARIABILI INTERNE
    // ════════════════════════════════════════════════════════════════

    private int _currentCoins = 0;
    private bool _isGameOver = false;
    private bool _hasWon = false;

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (_victoryImage != null)
        {
            _victoryImage.enabled = false;
        }
        else
        {
            Debug.LogError("Victory Image NON assegnata!");
        }

        if (_defeatImage != null)
        {
            _defeatImage.enabled = false;
        }
        else
        {
            Debug.LogError("Defeat Image NON assegnata!");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateCoinDisplay();

        if (_timerText == null || _coinText == null)
        {
            Debug.LogError("Assegna i componenti Text (Timer e Coins) nell'Inspector!");
        }

        if (_backToMenuButtonVictory != null)
        {
            _backToMenuButtonVictory.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Back To Menu Button (Victory) non assegnato!");
        }

        if (_restartButtonDefeat != null)
        {
            _restartButtonDefeat.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("Restart Button (Defeat) non assegnato!");
        }

        if (_backToMenuButtonDefeat != null)
        {
            _backToMenuButtonDefeat.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Back To Menu Button (Defeat) non assegnato!");
        }
    }

    void Update()
    {
        if (_isGameOver) return;

        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay(_timeRemaining);
        }
        else
        {
            _timeRemaining = 0;
            UpdateTimerDisplay(0);
            GameOver(false);
        }
    }

    /// <summary>
    /// Logica pura: Determina se il tempo è scaduto.
    /// ✅ TESTABILE: No dipendenze, solo logica booleana
    /// </summary>
    public bool IsTimeExpired(float timeRemaining)
    {
        return timeRemaining <= 0f;
    }

    /// <summary>
    /// Logica pura: Determina se il giocatore ha vinto.
    /// Vince se ha raccolto abbastanza monete.
    /// ✅ TESTABILE: Pura logica, no dipendenze
    /// </summary>
    public bool ShouldPlayerWin(int currentCoins, int requiredCoins)
    {
        return currentCoins >= requiredCoins;
    }

    /// <summary>
    /// Logica pura: Converte secondi in minuti e secondi.
    /// Ritorna un array [minutes, seconds].
    /// ✅ TESTABILE: Pura matematica
    /// </summary>
    public int[] ConvertTimeToMinutesSeconds(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return new int[] { minutes, seconds };
    }

    /// <summary>
    /// Logica pura: Formatta il tempo nel formato MM:SS.
    /// ✅ TESTABILE: Pura string formatting
    /// </summary>
    public string FormatTimeToString(int minutes, int seconds)
    {
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Logica pura: Determina se il timer dovrebbe essere rosso (urgenza).
    /// Rosso quando rimangono 15 secondi o meno.
    /// ✅ TESTABILE: Pura logica booleana
    /// </summary>
    public bool ShouldTimerBeRed(float timeRemaining)
    {
        return timeRemaining <= 15f;
    }

    /// <summary>
    /// Logica pura: Aggiunge monete e ritorna il nuovo contatore.
    /// Non modifica stato direttamente - ritorna il nuovo valore.
    /// ✅ TESTABILE: Pura aritmetica
    /// </summary>
    public int CalculateNewCoinCount(int currentCoins, int amountToAdd)
    {
        return currentCoins + amountToAdd;
    }

    /// <summary>
    /// Logica pura: Determina se aggiungere tempo bonus.
    /// Aggiungi tempo bonus SOLO se la moneta è speciale.
    /// ✅ TESTABILE: Pura logica condizionale
    /// </summary>
    public float CalculateNewTimeRemaining(float currentTime, float timeBonus, bool isSpecial)
    {
        if (isSpecial)
        {
            return currentTime + timeBonus;
        }
        return currentTime;
    }

    /// <summary>
    /// Aggiunge monete e gestisce la logica di vittoria.
    /// Delega i calcoli ai metodi puri (testabili).
    /// </summary>
    public void AddCoin(int amount, float timeBonus, bool isSpecial = false)
    {
        if (_isGameOver)
        {
            return;
        }

        // STEP 1: CALCOLA IL NUOVO CONTATORE DI MONETE (Logica pura)
        _currentCoins = CalculateNewCoinCount(_currentCoins, amount);

        // STEP 2: CALCOLA IL NUOVO TEMPO RIMANENTE (Logica pura)
        _timeRemaining = CalculateNewTimeRemaining(_timeRemaining, timeBonus, isSpecial);

        if (isSpecial)
        {
            Debug.Log($"⭐ MONETA SPECIALE RACCOLTA! +{timeBonus} secondi bonus!");
        }
        else
        {
            Debug.Log($"Moneta normale raccolta. +0 secondi.");
        }

        UpdateCoinDisplay();

        // STEP 3: VERIFICA CONDIZIONE DI VITTORIA (Logica pura)
        if (ShouldPlayerWin(_currentCoins, RequiredCoins))
        {
            GameOver(true);
        }
    }

    /// <summary>
    /// Aggiorna il display delle monete.
    /// </summary>
    private void UpdateCoinDisplay()
    {
        if (_coinText != null)
        {
            _coinText.text = "Monete: " + _currentCoins + " / " + RequiredCoins;
        }
    }

    /// <summary>
    /// Aggiorna il display del timer e il colore in base al tempo rimanente.
    /// </summary>
    void UpdateTimerDisplay(float timeToDisplay)
    {
        // STEP 1: CONVERTI TEMPO IN MINUTI/SECONDI (Logica pura)
        int[] timeComponents = ConvertTimeToMinutesSeconds(timeToDisplay);
        int minutes = timeComponents[0];
        int seconds = timeComponents[1];

        // STEP 2: FORMATTA LA STRINGA (Logica pura)
        string formattedTime = FormatTimeToString(minutes, seconds);
        _timerText.text = formattedTime;

        // STEP 3: DETERMINA IL COLORE (Logica pura)
        if (ShouldTimerBeRed(timeToDisplay))
        {
            _timerText.color = Color.red;
        }
        else
        {
            _timerText.color = Color.white;
        }
    }

    /// <summary>
    /// Gestisce la fine della partita (vittoria o sconfitta).
    /// Mette in pausa il gioco, mostra il cursore, e visualizza l'immagine appropriata.
    /// </summary>
    public void GameOver(bool hasWon)
    {
        if (_isGameOver)
        {
            return;
        }

        _isGameOver = true;
        _hasWon = hasWon;

        Debug.Log($"[GameManager] GameOver - HasWon: {hasWon}");

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (hasWon)
        {
            if (_victoryImage != null)
            {
                _victoryImage.gameObject.SetActive(true);
                Debug.Log("🎉 HAI VINTO!");
            }
        }
        else
        {
            if (_defeatImage != null)
            {
                _defeatImage.gameObject.SetActive(true);
                Debug.Log("💀 HAI PERSO!");
            }
        }
    }

    private void BackToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Ritorno al menu principale...");
        SceneManager.LoadScene(0);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Riavvio della partita...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UnlockDoor()
    {
        if (_exitDoor != null)
        {
            _exitDoor.Open();
        }
    }

    void OnDestroy()
    {
        if (_backToMenuButtonVictory != null)
        {
            _backToMenuButtonVictory.onClick.RemoveListener(BackToMenu);
        }

        if (_restartButtonDefeat != null)
        {
            _restartButtonDefeat.onClick.RemoveListener(RestartGame);
        }

        if (_backToMenuButtonDefeat != null)
        {
            _backToMenuButtonDefeat.onClick.RemoveListener(BackToMenu);
        }
    }
}