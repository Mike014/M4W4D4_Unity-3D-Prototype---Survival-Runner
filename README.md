# Unity 3D Prototype - Survival Runner

Questo progetto è un prototipo funzionale di un gioco 3D survival/runner, sviluppato utilizzando Primitive di Unity (Cubi, Sfere, Capsule) per testare le meccaniche di gameplay pure senza distrazioni grafiche.

Lo scopo del gioco è sopravvivere a un percorso a ostacoli, raccogliere monete per guadagnare tempo e punteggio, evitando trappole e torrette.

## Stato del Progetto
* Stato: Prototipo Funzionante (Alpha)
* Grafica: Placeholder (Primitive)
* Engine: Unity 3D
* **Architettura: Event System (refactored da Singleton)**

---

## 🔄 Refactoring: Da Singleton a Event System

### Il Problema Originale (Singleton)

L'architettura iniziale utilizzava il **Singleton Pattern** nel `GameManager`:

```csharp
// ❌ PRIMA: GameManager Singleton
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  // ← Accoppiamento forte
    
    public void AddCoin(int amount, float timeBonus, bool isSpecial) { ... }
    public void GameOver(bool hasWon) { ... }
}

// Coin e PlayerHealth dipendevano direttamente:
Coin.cs:           GameManager.Instance.AddCoin(...)
PlayerHealth.cs:   GameManager.Instance.GameOver(...)
```

**Problemi identificati:**
* ❌ **Accoppiamento diretto:** Coin e PlayerHealth conoscevano GameManager
* ❌ **Difficile da testare:** Singleton statico complica i test unitari
* ❌ **Non scalabile:** Aggiungere feature nuove (es: combo system) richiedeva di toccare GameManager
* ❌ **Dipendenze nascoste:** Difficile vedere quali script dipendevano da quale

### La Soluzione: Event System (Event-Driven Architecture)

Abbiamo refactorizzato l'architettura usando un **hub centrale basato su C# Events**, senza Singleton:

```csharp
// ✅ DOPO: GameEvents (MonoBehaviour senza Singleton)
public class GameEvents : MonoBehaviour
{
    public event Action<int, float, bool> OnCoinCollected;  // ← Evento pubblico
    public event Action<bool> OnGameOver;
    public event Action<float> OnTimeChanged;
    public event Action<int, int> OnCoinCountChanged;
    
    public void PublishCoinCollected(int amount, float timeBonus, bool isSpecial)
    {
        OnCoinCollected?.Invoke(amount, timeBonus, isSpecial);
    }
}
```

### Nuovo Flusso di Comunicazione

```
┌──────────┐
│   Coin   │  Raccolta
└────┬─────┘
     │
     ↓ GameEvents.PublishCoinCollected()
     │
┌────────────────────────────────────┐
│      GameEvents (Hub Centrale)     │
│      OnCoinCollected ✨            │
└────┬──────────────────────┬────────┘
     │ ascolta              │ ascolta
     ↓                      ↓
┌─────────────────┐    ┌──────────────┐
│  GameManager    │    │ ComboSystem* │
│ HandleCoin()    │    │ (future)     │
│ Aggiorna stato  │    │              │
└────────┬────────┘    └──────────────┘
         │
         ↓ PublishCoinCountChanged()
    ┌─────────┐
    │   UI    │ Ascolta e aggiorna display
    └─────────┘

* Facilissimo da aggiungere senza toccare GameManager
```

---

## Vantaggi del Refactoring

| Aspetto | Singleton ❌ | Event System ✅ |
|---------|----------|----------|
| **Accoppiamento** | Alto (dipendenze dirette) | Basso (dipendenze zero) |
| **Aggiungere feature** | Modificare GameManager | Aggiungere listener senza toccare nulla |
| **Testabilità** | Difficile (Singleton statico) | Facile (FindObjectOfType) |
| **Scalabilità** | Scarsa (non scala bene) | Ottima (event-driven) |
| **Numero di listener** | 1 (sempre GameManager) | Illimitati (chiunque voglia ascoltare) |

### Esempio: Aggiungere ComboSystem

**Con Singleton:**
```csharp
// Devi modificare GameManager.cs
public void AddCoin(...) {
    // ... codice nuovo di combo ...
    _comboCount++;
}
```

**Con Event System:**
```csharp
// Crei ComboSystem.cs, GameManager rimane invariato!
public class ComboSystem : MonoBehaviour
{
    void Start()
    {
        FindObjectOfType<GameEvents>().OnCoinCollected += HandleCombo;
    }
    
    private void HandleCombo(int amount, float timeBonus, bool isSpecial)
    {
        _comboCount++;
        Debug.Log($"COMBO x{_comboCount}!");
    }
}
```

**Risultato:** Zero modifiche al codice esistente. ✅ **Open/Closed Principle**

---

## Implementazione Tecnica

### File Modificati

1. **GameEvents_NoSingleton.cs** (NUOVO)
   - Hub centrale degli eventi
   - Niente Singleton, niente stato
   - Trovato tramite `FindObjectOfType<GameEvents>()`

2. **GameManager_NoSingleton.cs** (REFACTORED)
   - ❌ Rimosso: `public static GameManager Instance`
   - ✅ Aggiunto: `OnEnable()/OnDisable()` per subscribe/unsubscribe agli eventi
   - ✅ Aggiunto: `HandleCoinCollected()` e `HandleGameOver()` come event handlers
   - Logica pura rimane identica e testabile

3. **Coin_NoSingleton.cs** (REFACTORED)
   - ❌ Rimosso: `GameManager.Instance.AddCoin()`
   - ✅ Aggiunto: `GameEvents.PublishCoinCollected()`
   - Coin non conosce più GameManager

4. **PlayerHealth_NoSingleton.cs** (REFACTORED)
   - ❌ Rimosso: `GameManager.Instance.GameOver()`
   - ✅ Aggiunto: `GameEvents.PublishGameOver()`
   - PlayerHealth non conosce più GameManager

5. **GameManagerTests_Updated.cs** (AGGIORNATO)
   - Setup modificato: crea GameEvents prima di GameManager
   - Tutti i test rimangono identici (logica pura invariata)
   - Solo 5 linee di codice cambiate nel setup

---

## FunzionalitÀ Implementate

### Player Controller & Fisica
* Movimento Fisico: Implementato tramite Rigidbody in FixedUpdate per garantire interazioni fisiche solide.
* Salto: Logica di GroundCheck tramite Physics.CheckSphere per prevenire salti infiniti.
* Recoil System: Quando il giocatore colpisce un muro o un ostacolo, perde temporaneamente il controllo e viene respinto indietro.

### Interazione Ambientale
* Boundaries (Muri di Confine): I muri perimetrali possiedono una logica di "Pushback". Se il giocatore tenta di uscire dall'area:
    * Viene applicata una forza opposta (AddForce).
    * Non viene inflitto danno (solo spinta fisica).
* Obstacles (Ostacoli):
    * Possiedono Basic Animations (es. movimento o rotazione semplice).
    * Infliggono danno al contatto, riducendo la salute del giocatore.

### Game Loop & Manager (Event-Driven)
* **GameEvents:** Hub centrale basato su C# Events (no Singleton)
* **GameManager:** Ascolta gli eventi di Coin e PlayerHealth, aggiorna lo stato interno
* **Sistema Monete:**
    * Le monete pubblicano un evento quando raccolte
    * GameManager ascolta e aggiorna il contatore
    * UI ascolta e aggiorna il display
* **Timer System:** Countdown progressivo. Se arriva a 0, scatta il Game Over (Time.timeScale = 0)

### AI (Torrette)
* Sistema di puntamento automatico verso il player (LookAt).
* Logica di sparo a intervalli regolari con proiettili fisici.

---

## Work In Progress (Da Implementare)

Attualmente il core loop è completo, ma mancano alcuni elementi di rifinitura finale:

* Exit Door (Uscita): La logica di sblocco (RequiredCoins) è presente nel codice, ma l'oggetto fisico "Porta" e la sua interazione non sono ancora stati posizionati nella scena.
* Animazioni Avanzate: Le animazioni attuali sono procedurali o base. Verranno aggiunti Animator Controller completi per la porta e il player.
* Modelli 3D: Sostituzione delle primitive con asset grafici definitivi.

---

## Architettura Tecnica

Il progetto segue principi di programmazione modulare e best practices professionali:

* **Event-Driven Architecture:** Utilizzo di C# Events per decoupling tra script
* **Logica Pura Separata:** Metodi testabili senza dipendenze esterne (IsTimeExpired, ShouldPlayerWin, ConvertTimeToMinutesSeconds, etc.)
* **UnityEvents:** Il sistema PlayerHealth utilizza eventi per notificare danni e morte, disaccoppiando la logica della salute dalla UI
* **UI Legacy:** Implementazione di un'interfaccia utente funzionale per Timer e Contatore Monete
* **FindObjectOfType Pattern:** Ricerca runtime di componenti senza Singleton statico

---

## Comandi

| Tasto | Azione |
| :--- | :--- |
| W, A, S, D | Movimento |
| Spazio | Salto |
| Mouse | Rotazione Telecamera (Orbit) |

---

## Testing

I test unitari rimangono **99% identici** perché testano **logica pura** che non dipende da Singleton o Event System:

```csharp
[SetUp]
public void Setup()
{
    // Crea GameEvents PRIMA di GameManager
    _eventManagerObj = new GameObject("_EventManager");
    _eventManagerObj.AddComponent<GameEvents>();
    
    _gameManagerObj = new GameObject("GameManager");
    _gameManager = _gameManagerObj.AddComponent<GameManager>();
}

// Tutti i test rimangono identici: testano logica pura
[Test]
public void ShouldPlayerWin_ReturnsTrue_WhenCoinsEqualRequired()
{
    bool result = _gameManager.ShouldPlayerWin(5, 5);
    Assert.IsTrue(result);
}
```

---

## Principi Architetturali Applicati

✅ **SOLID Principles:**
- **S**ingle Responsibility: Ogni script ha una responsabilità
- **O**pen/Closed: Aggiungere feature senza modificare codice esistente
- **L**iskov Substitution: Events permettono intercambiabilità
- **I**nterface Segregation: Piccoli listener specializzati
- **D**ependency Inversion: Dipendenza da astrazioni (events) non da implementazioni (Singleton)

✅ **Clean Code:**
- Logica pura separata da side effects
- Nomi descrittivi e self-documenting
- Commenti che spiegano il "perché" non il "cosa"

✅ **Game Development Best Practices:**
- Event-driven communication per game systems
- Decoupled architecture per scalabilità
- Testability embedded fin dall'inizio

---

**Progetto sviluppato a scopo didattico per Master in Game Development.**

