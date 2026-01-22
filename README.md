# Unity 3D Prototype - Survival Runner

Questo progetto è un prototipo funzionale di un gioco 3D survival/runner, sviluppato utilizzando Primitive di Unity (Cubi, Sfere, Capsule) per testare le meccaniche di gameplay pure senza distrazioni grafiche.

Lo scopo del gioco è sopravvivere a un percorso a ostacoli, raccogliere monete per guadagnare tempo e punteggio, evitando trappole e torrette.

## Stato del Progetto
* Stato: Prototipo Funzionante (Alpha)
* Grafica: Placeholder (Primitive)
* Engine: Unity 3D

## Funzionalità Implementate

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

### Game Loop & Manager
* GameManager (Singleton): Gestisce centralmente il timer, il punteggio e gli stati di vittoria/sconfitta.
* Sistema Monete:
    * Le monete sono trigger che, una volta raccolti, comunicano con il GameManager.
    * Gestiscono punteggio e Bonus Tempo (essenziale per completare percorsi lunghi).
* Timer System: Countdown progressivo. Se arriva a 0, scatta il Game Over (Time.timeScale = 0).

### AI (Torrette)
* Sistema di puntamento automatico verso il player (LookAt).
* Logica di sparo a intervalli regolari con proiettili fisici.

## Work In Progress (Da Implementare)

Attualmente il core loop è completo, ma mancano alcuni elementi di rifinitura finale:

* Exit Door (Uscita): La logica di sblocco (RequiredCoins) è presente nel codice, ma l'oggetto fisico "Porta" e la sua interazione non sono ancora stati posizionati nella scena.
* Animazioni Avanzate: Le animazioni attuali sono procedurali o base. Verranno aggiunti Animator Controller completi per la porta e il player.
* Modelli 3D: Sostituzione delle primitive con asset grafici definitivi.

## Architettura Tecnica

Il progetto segue principi di programmazione modulare:
* Singleton Pattern: Utilizzato nel GameManager per facilitare la comunicazione tra oggetti (es. Moneta -> Manager).
* UnityEvents: Il sistema PlayerHealth utilizza eventi per notificare i danni e la morte, disaccoppiando la logica della salute dalla UI.
* UI Legacy: Implementazione di un'interfaccia utente funzionale per Timer e Contatore Monete.

## Comandi

| Tasto | Azione |
| :--- | :--- |
| W, A, S, D | Movimento |
| Spazio | Salto |
| Mouse | Rotazione Telecamera (Orbit) |

---
Progetto sviluppato a scopo didattico per modulo Unity Development.