using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class UIManagerTests
{
    private UIManager _uiManager;
    private GameObject _uiManagerObj;
    private GameObject _gameEventsObj;

    // Pannelli
    private GameObject _victoryPanelObj;
    private GameObject _defeatPanelObj;
    private Image _victoryImage;
    private Image _defeatImage;

    // Bottoni
    private Button _backToMenuButtonVictory;
    private Button _restartButtonDefeat;
    private Button _backToMenuButtonDefeat;

    // Testi
    private Text _timerText;
    private Text _coinText;

    // ════════════════════════════════════════════════════════════════
    // SETUP & TEARDOWN
    // ════════════════════════════════════════════════════════════════

    [SetUp]
    public void Setup()
    {
        // 1. Crea GameEvents (il singleton deve esistere prima di UIManager)
        _gameEventsObj = new GameObject("GameEvents");
        var gameEvents = _gameEventsObj.AddComponent<GameEvents>();

        typeof(GameEvents)
            .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, gameEvents);

        // 2. Crea UIManager
        _uiManagerObj = new GameObject("UIManager");
        _uiManager = _uiManagerObj.AddComponent<UIManager>();

        // 3. Crea i pannelli vittoria/sconfitta
        _victoryPanelObj = new GameObject("VictoryPanel");
        _victoryImage = _victoryPanelObj.AddComponent<Image>();

        _defeatPanelObj = new GameObject("DefeatPanel");
        _defeatImage = _defeatPanelObj.AddComponent<Image>();

        // 4. Crea i bottoni
        _backToMenuButtonVictory = new GameObject("BackToMenuVictory").AddComponent<Button>();
        _restartButtonDefeat = new GameObject("RestartDefeat").AddComponent<Button>();
        _backToMenuButtonDefeat = new GameObject("BackToMenuDefeat").AddComponent<Button>();

        // 5. Crea i Text
        _timerText = new GameObject("TimerText").AddComponent<Text>();
        _coinText = new GameObject("CoinText").AddComponent<Text>();

        // 6. Inietta i campi privati tramite reflection (come PlayerControllerTests)
        SetPrivateField("_victoryImage", _victoryImage);
        SetPrivateField("_defeatImage", _defeatImage);
        SetPrivateField("_backToMenuButtonVictory", _backToMenuButtonVictory);
        SetPrivateField("_restartButtonDefeat", _restartButtonDefeat);
        SetPrivateField("_backToMenuButtonDefeat", _backToMenuButtonDefeat);
        SetPrivateField("_timerText", _timerText);
        SetPrivateField("_coinText", _coinText);

        // 7. Simula Start() — registra i listener sui bottoni e nasconde i pannelli
        InvokePrivateMethod("Start");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_uiManagerObj);
        Object.DestroyImmediate(_victoryPanelObj);
        Object.DestroyImmediate(_defeatPanelObj);
        Object.DestroyImmediate(_backToMenuButtonVictory?.gameObject);
        Object.DestroyImmediate(_restartButtonDefeat?.gameObject);
        Object.DestroyImmediate(_backToMenuButtonDefeat?.gameObject);
        Object.DestroyImmediate(_timerText?.gameObject);
        Object.DestroyImmediate(_coinText?.gameObject);

        // Reset del singleton GameEvents per evitare contaminazione tra i test
        typeof(GameEvents)
            .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, null);
        Object.DestroyImmediate(_gameEventsObj);
    }

    // ════════════════════════════════════════════════════════════════
    // HELPER: Reflection
    // ════════════════════════════════════════════════════════════════

    private void SetPrivateField(string fieldName, object value)
    {
        typeof(UIManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_uiManager, value);
    }

    private void InvokePrivateMethod(string methodName, params object[] args)
    {
        typeof(UIManager)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_uiManager, args);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 1: Visibilità iniziale dei pannelli (il bug originale)
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifica che il pannello vittoria parta INATTIVO.
    /// BUG: in precedenza si usava enabled=false (nascondeva solo l'Image)
    /// invece di SetActive(false) — rendendo SetActive(true) un no-op al game over.
    /// </summary>
    [Test]
    public void Start_VictoryPanel_StartsInactive()
    {
        Assert.IsFalse(
            _victoryPanelObj.activeSelf,
            "Il pannello vittoria deve essere inattivo (SetActive=false) all'avvio"
        );
    }

    /// <summary>
    /// Verifica che il pannello sconfitta parta INATTIVO.
    /// Stesso bug del pannello vittoria.
    /// </summary>
    [Test]
    public void Start_DefeatPanel_StartsInactive()
    {
        Assert.IsFalse(
            _defeatPanelObj.activeSelf,
            "Il pannello sconfitta deve essere inattivo (SetActive=false) all'avvio"
        );
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 2: HandleGameOver — attivazione corretta dei pannelli
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Vittoria: il pannello vittoria deve diventare ATTIVO.
    /// Prima del fix SetActive(true) era no-op perché il GO era già attivo.
    /// </summary>
    [Test]
    public void HandleGameOver_Victory_ShowsVictoryPanel()
    {
        InvokePrivateMethod("HandleGameOver", true);

        Assert.IsTrue(
            _victoryPanelObj.activeSelf,
            "HandleGameOver(true) deve attivare il pannello vittoria"
        );
    }

    /// <summary>
    /// Vittoria: il pannello sconfitta deve rimanere INATTIVO.
    /// </summary>
    [Test]
    public void HandleGameOver_Victory_DoesNotShowDefeatPanel()
    {
        InvokePrivateMethod("HandleGameOver", true);

        Assert.IsFalse(
            _defeatPanelObj.activeSelf,
            "HandleGameOver(true) NON deve attivare il pannello sconfitta"
        );
    }

    /// <summary>
    /// Sconfitta: il pannello sconfitta deve diventare ATTIVO.
    /// </summary>
    [Test]
    public void HandleGameOver_Defeat_ShowsDefeatPanel()
    {
        InvokePrivateMethod("HandleGameOver", false);

        Assert.IsTrue(
            _defeatPanelObj.activeSelf,
            "HandleGameOver(false) deve attivare il pannello sconfitta"
        );
    }

    /// <summary>
    /// Sconfitta: il pannello vittoria deve rimanere INATTIVO.
    /// </summary>
    [Test]
    public void HandleGameOver_Defeat_DoesNotShowVictoryPanel()
    {
        InvokePrivateMethod("HandleGameOver", false);

        Assert.IsFalse(
            _victoryPanelObj.activeSelf,
            "HandleGameOver(false) NON deve attivare il pannello vittoria"
        );
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 3: Interazione bottoni → GameEvents
    // Questi test verificano che i bottoni pubblichino gli eventi corretti.
    // Se il bottone non è connesso, o pubblica l'evento sbagliato, falliscono.
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Il bottone BackToMenu del pannello VITTORIA deve pubblicare OnBackToMenuRequested.
    /// </summary>
    [Test]
    public void BackToMenuButtonVictory_OnClick_PublishesBackToMenuEvent()
    {
        bool eventFired = false;
        GameEvents.Instance.OnBackToMenuRequested += () => eventFired = true;

        _backToMenuButtonVictory.onClick.Invoke();

        Assert.IsTrue(
            eventFired,
            "Il click su BackToMenu (Vittoria) deve pubblicare OnBackToMenuRequested"
        );
    }

    /// <summary>
    /// Il bottone BackToMenu del pannello SCONFITTA deve pubblicare OnBackToMenuRequested.
    /// </summary>
    [Test]
    public void BackToMenuButtonDefeat_OnClick_PublishesBackToMenuEvent()
    {
        bool eventFired = false;
        GameEvents.Instance.OnBackToMenuRequested += () => eventFired = true;

        _backToMenuButtonDefeat.onClick.Invoke();

        Assert.IsTrue(
            eventFired,
            "Il click su BackToMenu (Sconfitta) deve pubblicare OnBackToMenuRequested"
        );
    }

    /// <summary>
    /// Il bottone Restart del pannello SCONFITTA deve pubblicare OnRestartRequested.
    /// </summary>
    [Test]
    public void RestartButtonDefeat_OnClick_PublishesRestartEvent()
    {
        bool eventFired = false;
        GameEvents.Instance.OnRestartRequested += () => eventFired = true;

        _restartButtonDefeat.onClick.Invoke();

        Assert.IsTrue(
            eventFired,
            "Il click su Restart (Sconfitta) deve pubblicare OnRestartRequested"
        );
    }

    /// <summary>
    /// Il bottone BackToMenu NON deve pubblicare OnRestartRequested per errore.
    /// Verifica che i bottoni non siano collegati all'evento sbagliato.
    /// </summary>
    [Test]
    public void BackToMenuButton_OnClick_DoesNotPublishRestartEvent()
    {
        bool wrongEventFired = false;
        GameEvents.Instance.OnRestartRequested += () => wrongEventFired = true;

        _backToMenuButtonVictory.onClick.Invoke();

        Assert.IsFalse(
            wrongEventFired,
            "BackToMenu NON deve pubblicare OnRestartRequested"
        );
    }

    /// <summary>
    /// Il bottone Restart NON deve pubblicare OnBackToMenuRequested per errore.
    /// </summary>
    [Test]
    public void RestartButton_OnClick_DoesNotPublishBackToMenuEvent()
    {
        bool wrongEventFired = false;
        GameEvents.Instance.OnBackToMenuRequested += () => wrongEventFired = true;

        _restartButtonDefeat.onClick.Invoke();

        Assert.IsFalse(
            wrongEventFired,
            "Restart NON deve pubblicare OnBackToMenuRequested"
        );
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 4: ConvertTimeToMinutesSeconds()
    // ════════════════════════════════════════════════════════════════

    [Test]
    public void ConvertTimeToMinutesSeconds_65Seconds_Returns1Min5Sec()
    {
        int[] result = _uiManager.ConvertTimeToMinutesSeconds(65f);

        Assert.AreEqual(1, result[0], "Minuti devono essere 1");
        Assert.AreEqual(5, result[1], "Secondi devono essere 5");
    }

    [Test]
    public void ConvertTimeToMinutesSeconds_150Seconds_Returns2Min30Sec()
    {
        int[] result = _uiManager.ConvertTimeToMinutesSeconds(150f);

        Assert.AreEqual(2, result[0], "Minuti devono essere 2");
        Assert.AreEqual(30, result[1], "Secondi devono essere 30");
    }

    /// <summary>
    /// Verifica che i decimali vengano troncati (FloorToInt), non arrotondati.
    /// 45.9 → 45, non 46.
    /// </summary>
    [Test]
    public void ConvertTimeToMinutesSeconds_TruncatesDecimals_NotRounds()
    {
        int[] result = _uiManager.ConvertTimeToMinutesSeconds(45.9f);

        Assert.AreEqual(0, result[0]);
        Assert.AreEqual(45, result[1], "45.9 secondi devono diventare 45 (troncato, non 46)");
    }

    [Test]
    public void ConvertTimeToMinutesSeconds_ZeroSeconds_ReturnsBothZero()
    {
        int[] result = _uiManager.ConvertTimeToMinutesSeconds(0f);

        Assert.AreEqual(0, result[0]);
        Assert.AreEqual(0, result[1]);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 5: ShouldTimerBeRed()
    // ════════════════════════════════════════════════════════════════

    [Test]
    public void ShouldTimerBeRed_ReturnsTrue_AtExactly15Seconds()
    {
        Assert.IsTrue(_uiManager.ShouldTimerBeRed(15f), "Deve essere rosso a esattamente 15s");
    }

    [Test]
    public void ShouldTimerBeRed_ReturnsTrue_Below15Seconds()
    {
        Assert.IsTrue(_uiManager.ShouldTimerBeRed(5f), "Deve essere rosso sotto i 15s");
    }

    [Test]
    public void ShouldTimerBeRed_ReturnsTrue_AtZeroSeconds()
    {
        Assert.IsTrue(_uiManager.ShouldTimerBeRed(0f), "Deve essere rosso a 0s");
    }

    [Test]
    public void ShouldTimerBeRed_ReturnsFalse_Above15Seconds()
    {
        Assert.IsFalse(_uiManager.ShouldTimerBeRed(16f), "Non deve essere rosso sopra i 15s");
    }

    [Test]
    public void ShouldTimerBeRed_ReturnsFalse_AtFullTime()
    {
        Assert.IsFalse(_uiManager.ShouldTimerBeRed(120f), "Non deve essere rosso a tempo pieno");
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 6: FormatTimeToString()
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifica il padding con zero: 1 minuto e 5 secondi → "01:05" (non "1:5").
    /// </summary>
    [Test]
    public void FormatTimeToString_AddsPaddingCorrectly()
    {
        string result = _uiManager.FormatTimeToString(1, 5);

        Assert.AreEqual("01:05", result, "Deve aggiungere zero-padding: '01:05'");
    }

    [Test]
    public void FormatTimeToString_ZeroTime_Returns0000()
    {
        string result = _uiManager.FormatTimeToString(0, 0);

        Assert.AreEqual("00:00", result);
    }

    [Test]
    public void FormatTimeToString_DoubleDigits_NoExtraPadding()
    {
        string result = _uiManager.FormatTimeToString(10, 30);

        Assert.AreEqual("10:30", result, "Doppia cifra non deve aggiungere padding extra");
    }

    [Test]
    public void FormatTimeToString_ContainsColon()
    {
        string result = _uiManager.FormatTimeToString(2, 45);

        Assert.IsTrue(result.Contains(":"), "Il formato deve contenere i due punti");
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 7: HandleCoinCountChanged() — aggiornamento testo monete
    // ════════════════════════════════════════════════════════════════

    [Test]
    public void HandleCoinCountChanged_UpdatesCoinText_Correctly()
    {
        InvokePrivateMethod("HandleCoinCountChanged", 3, 5);

        Assert.AreEqual("Monete : 3 / 5", _coinText.text);
    }

    [Test]
    public void HandleCoinCountChanged_WithMaxCoins_ShowsCorrectText()
    {
        InvokePrivateMethod("HandleCoinCountChanged", 5, 5);

        Assert.AreEqual("Monete : 5 / 5", _coinText.text);
    }

    [Test]
    public void HandleCoinCountChanged_WithZeroCoins_ShowsCorrectText()
    {
        InvokePrivateMethod("HandleCoinCountChanged", 0, 5);

        Assert.AreEqual("Monete : 0 / 5", _coinText.text);
    }
}
