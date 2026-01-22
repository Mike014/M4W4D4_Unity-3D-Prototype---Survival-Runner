using UnityEngine;

/// <summary>
/// Gestisce una camera che orbita intorno a un target (il player).
/// La camera segue il mouse per ruotare, con supporto per collisioni con i muri
/// e smoothing per evitare scatti improvvisi.
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    // Transform del player attorno al quale la camera orbita
    [SerializeField] private Transform _target;
    
    [Header("Orbit Settings")]
    // Velocità di rotazione della camera rispetto al movimento del mouse (moltiplicatore)
    [SerializeField] private float _mouseSensitivity = 2f; 
    // Distanza desiderata tra camera e player (quando non ci sono muri)
    [SerializeField] private float _distance = 5f;
    // Limite inferiore dell'angolo verticale (guardo verso l'alto massimo)
    [SerializeField] private float _minVerticalAngle = -20f;
    // Limite superiore dell'angolo verticale (guardo verso il basso massimo)
    [SerializeField] private float _maxVerticalAngle = 70f;
    
    [Header("Collision & Smoothing")]
    // Abilita il raycast per evitare che la camera penetri i muri
    [SerializeField] private bool _enableCollision = true;
    // Layer mask che identifica quali oggetti bloccano la camera
    [SerializeField] private LayerMask _collisionLayers; 
    // Spazio cuscinetto tra camera e muro (evita che la camera tocchi i collider)
    [SerializeField] private float _collisionPadding = 0.2f;
    // Tempo di smorzamento per il movimento della camera (più alto = più lento/smooth)
    [SerializeField] private float _positionSmoothTime = 0.12f;

    // Angolo orizzontale (rotazione sinistra/destra intorno al player) in gradi
    private float _currentX = 0f;
    // Angolo verticale (rotazione su/giù) in gradi
    private float _currentY = 0f;
    // Velocità interna usata da SmoothDamp per calcolare l'interpolazione
    // (richiesto come parametro ref, non modificare direttamente)
    private Vector3 _currentVelocity = Vector3.zero;

    void Start()
    {
        // SETUP INPUT DEL MOUSE
        // Nasconde il cursore e lo blocca al centro dello schermo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // INIZIALIZZAZIONE ANGOLI
        // Leggi gli angoli attuali della camera per evitare "jump" iniziale
        Vector3 angles = transform.eulerAngles;
        // eulerAngles.y è la rotazione orizzontale (yaw)
        _currentX = angles.y;
        // eulerAngles.x è la rotazione verticale (pitch), negata perché Unity usa convenzione diversa
        _currentY = angles.x;
    }

    void Update()
    {
        // Raccogli l'input del mouse PRIMA di qualsiasi calcolo
        // (massima reattività ai controlli)
        HandleInput();
        
        // SBLOCCO CURSORE CON ESC
        // Permette al giocatore di uscire da LookMode bloccato se necessario
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        // LateUpdate() viene eseguito DOPO Update() di tutti gli altri script
        // Garantisce che il player si sia già mosso prima di aggiornare la camera
        // (altrimenti la camera seguirebbe la posizione OLD del player)
        
        // Protezione: se il target non esiste (player distrutto), non fare nulla
        if (_target == null) return;
        
        // Calcola e applica la nuova posizione/rotazione della camera
        UpdateCameraTransform();
    }

    /// <summary>
    /// Legge l'input del mouse e aggiorna gli angoli di orbita.
    /// Clamppa l'angolo verticale per evitare che la camera vada troppo su/giù.
    /// </summary>
    private void HandleInput()
    {
        // ROTAZIONE ORIZZONTALE (Sinistra/Destra)
        // GetAxis("Mouse X") ritorna il movimento orizzontale del mouse dal frame precedente
        // Non necessita Time.deltaTime: il valore è già un delta frame-based (varia con deltaTime)
        _currentX += Input.GetAxis("Mouse X") * _mouseSensitivity;
        
        // ROTAZIONE VERTICALE (Su/Giù)
        // Negato perché movimento mouse su = angolo pitch negativo (guardare su)
        _currentY -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        
        // CLAMP VERTICALE
        // Limita l'angolo verticale in una range sensata
        // Evita che il giocatore possa guardare dietro la testa (confusione)
        _currentY = Mathf.Clamp(_currentY, _minVerticalAngle, _maxVerticalAngle);
    }

    /// <summary>
    /// Calcola la nuova posizione e rotazione della camera.
    /// Applica collisioni se abilitate, poi usa SmoothDamp per un movimento fluido.
    /// </summary>
    private void UpdateCameraTransform()
    {
        // STEP 1: CALCOLA LA ROTAZIONE DESIDERATA
        // Crea un quaternione partendo dagli angoli Euler (pitch, yaw, roll)
        // roll = 0 perché la camera non ruota mai su se stessa
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // STEP 2: CALCOLA LA POSIZIONE TEORICA (SENZA COLLISIONI)
        // Moltiplica il quaternione per un vettore "avanti" negativo (reverse Z)
        // Questo ritorna un vettore che punta nella direzione opposta allo sguardo della camera
        Vector3 direction = rotation * new Vector3(0, 0, -_distance);
        // La posizione desiderata è il target + il vettore direction
        Vector3 desiredPosition = _target.position + direction;

        // STEP 3: GESTIONE COLLISIONI CON RAYCAST
        // Controlla se c'è qualcosa tra il target e la posizione desiderata
        float finalDistance = _distance;
        if (_enableCollision)
        {
            // CheckCameraCollision ritorna la distanza da usare (ridotta se c'è un ostacolo)
            finalDistance = CheckCameraCollision(_target.position, desiredPosition);
        }

        // STEP 4: RICALCOLA POSIZIONE FINALE CORRETTA
        // Usa la distanza finale (potenzialmente ridotta dal raycast) per calcolare la posizione vera
        // direction.normalized: assicura che la lunghezza sia esattamente 1
        // (altrimenti un vettore arbitrario + distanza = risultato impreciso)
        Vector3 finalPosition = _target.position + (direction.normalized * finalDistance);

        // STEP 5: APPLICA SMOOTHING SULLA POSIZIONE
        // SmoothDamp interpola SMOOTH tra posizione attuale e posizione finale
        // Parametri:
        //   - current: posizione attuale della camera
        //   - target: posizione desiderata
        //   - velocity: ref parameter che traccia la velocità (SmoothDamp la modifica internamente)
        //   - smoothTime: tempo approssimativo per raggiungere il target (0.12s)
        // Questo è meglio di Lerp perché evita overshooting e jitter
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            finalPosition, 
            ref _currentVelocity, 
            _positionSmoothTime
        );
        
        // PUNTA LA CAMERA VERSO IL TARGET
        // LookAt fa ruotare la camera per guardare un punto specifico
        // Vector3.up * 1.5f: solleva il punto di sguardo di 1.5 unità
        // (rende il frame più naturale, guardando leggermente in basso verso il player)
        transform.LookAt(_target.position + Vector3.up * 1.5f);
    }

    /// <summary>
    /// Controlla se la camera collide con qualcosa usando raycast.
    /// Ritorna la distanza sicura dalla camera al target (ridotta se c'è un ostacolo).
    /// </summary>
    /// <param name="startPos">Posizione del target (origine del raycast)</param>
    /// <param name="endPos">Posizione desiderata della camera (per calcolare la direzione)</param>
    /// <returns>Distanza sicura da usare (massimo _distance, minimo 0.5f)</returns>
    private float CheckCameraCollision(Vector3 startPos, Vector3 endPos)
    {
        RaycastHit hit;
        // Raycast un raggio dal target verso la posizione desiderata della camera
        // Parametri:
        //   - startPos: origine del raycast (il target)
        //   - endPos - startPos: direzione del raycast (verso la camera)
        //   - hit: output che contiene info sulla collisione
        //   - _distance: lunghezza massima del raycast
        //   - _collisionLayers: layer mask per filtrare cosa può collidere
        if (Physics.Raycast(startPos, endPos - startPos, out hit, _distance, _collisionLayers))
        {
            // Se colpiamo un ostacolo, accorcia la distanza della camera
            // hit.distance: distanza dal startPos al punto di collisione
            // - _collisionPadding: spazio cuscinetto per non toccare il collider
            // Clamp: assicura che la distanza rimanga tra 0.5f e _distance
            // (evita che la camera vada troppo vicino o troppo lontano)
            return Mathf.Clamp(hit.distance - _collisionPadding, 0.5f, _distance);
        }
        // Se non c'è collisione, usa la distanza massima desiderata
        return _distance;
    }
}