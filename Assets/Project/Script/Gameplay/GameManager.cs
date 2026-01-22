using UnityEngine;
using UnityEngine.UI; // Necessario per il componente Text Legacy

public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _coinText;  // NUOVO: Slot per il testo delle monete

    [Header("Timer Settings")]
    [SerializeField] private float _timeRemaining = 120f;

    [Header("Level Settings")]
    [SerializeField] private Door _exitDoor;
    public int RequiredCoins = 5;
    
    private int _currentCoins = 0;
    private bool _isGameOver = false;

    public static GameManager Instance;

    private void Awake()
    {
        // Singleton Pattern per l'accesso globale
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

        // Inizializziamo la UI
        UpdateCoinDisplay();
        
        if (_timerText == null || _coinText == null)
        {
            Debug.LogError("Assegna i componenti Text (Timer e Coins) nell'Inspector!");
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
            GameOver();
        }
    }

    // Funzione chiamata da Coin.cs quando viene raccolta una moneta
    public void AddCoin(int amount, float timeBonus)
    {
        _currentCoins += amount;
        _timeRemaining += timeBonus; // Aggiunge bonus tempo

        UpdateCoinDisplay(); // Aggiorna il testo a schermo

        if (_currentCoins >= RequiredCoins)
        {
            UnlockDoor();
        }
    }

    // NUOVO: Gestisce l'aggiornamento del testo "X / Y"
    private void UpdateCoinDisplay()
    {
        if (_coinText != null)
        {
            _coinText.text = "Monete: " + _currentCoins + " / " + RequiredCoins;
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timeToDisplay <= 15f) _timerText.color = Color.red;
    }

    void GameOver()
    {
        _isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("Tempo Scaduto!");
    }

    private void UnlockDoor()
    {
        if (_exitDoor != null) _exitDoor.Open();
    }
}