using UnityEngine;

/// <summary>
/// Gestisce una camera che orbita intorno a un target (il player).
/// La camera segue il mouse per ruotare, con supporto per collisioni con i muri
/// e smoothing per evitare scatti improvvisi.
/// 
/// DESIGN: La logica di aggiornamento angoli (UpdateAngles) è separata dall'input
/// per consentire testing senza dipendenze da Input.GetAxis().
/// Questo rende il codice più testabile e modulare.
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    // Transform del player attorno al quale la camera orbita
    [SerializeField] private Transform _target;
    
    [Header("Orbit Settings")]
    // VelocitÃ  di rotazione della camera rispetto al movimento del mouse (moltiplicatore)
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
    // Tempo di smorzamento per il movimento della camera (piÃ¹ alto = piÃ¹ lento/smooth)
    [SerializeField] private float _positionSmoothTime = 0.12f;

    // Angolo orizzontale (rotazione sinistra/destra intorno al player) in gradi
    private float _currentX = 0f;
    // Angolo verticale (rotazione su/giÃ¹) in gradi
    private float _currentY = 0f;
    // VelocitÃ  interna usata da SmoothDamp per calcolare l'interpolazione
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
        // (massima reattivitÃ  ai controlli)
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
        // Garantisce che il player si sia giÃ  mosso prima di aggiornare la camera
        // (altrimenti la camera seguirebbe la posizione OLD del player)
        
        // Protezione: se il target non esiste (player distrutto), non fare nulla
        if (_target == null) return;
        
        // Calcola e applica la nuova posizione/rotazione della camera
        UpdateCameraTransform();
    }

    /// <summary>
    /// Legge l'input del mouse e delega la logica di aggiornamento angoli a UpdateAngles().
    /// Questa separazione rende il codice testabile mantenendo Update() pulito.
    /// </summary>
    private void HandleInput()
    {
        // Leggi l'input dal mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        // Delega la logica di aggiornamento a un metodo puro (testabile)
        UpdateAngles(mouseX, mouseY);
    }

    /// <summary>
    /// Logica pura per aggiornare gli angoli di orbita.
    /// Non dipende da Input o Transform, quindi è facilmente testabile.
    /// Parametri:
    ///   - mouseX: movimento orizzontale del mouse (delta frame-based)
    ///   - mouseY: movimento verticale del mouse (delta frame-based)
    /// </summary>
    private void UpdateAngles(float mouseX, float mouseY)
    {
        // ROTAZIONE ORIZZONTALE (Sinistra/Destra)
        // Aggiunge il movimento orizzontale del mouse all'angolo X
        // moltiplicato per la sensibilità di rotazione
        _currentX += mouseX * _mouseSensitivity;
        
        // ROTAZIONE VERTICALE (Su/GiÃ¹)
        // Negato perchÃ© movimento mouse su = angolo pitch negativo (guardare su)
        // Input.GetAxis("Mouse Y") ritorna valori positivi quando muovi il mouse in su,
        // ma vogliamo che guardare in su DIMINUISCA _currentY (angolo negativo)
        _currentY -= mouseY * _mouseSensitivity;
        
        // CLAMP VERTICALE
        // Limita l'angolo verticale in una range sensata
        // Evita che il giocatore possa guardare dietro la testa (confusione visuale)
        // Se _currentY < _minVerticalAngle: lo mette a _minVerticalAngle
        // Se _currentY > _maxVerticalAngle: lo mette a _maxVerticalAngle
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
        // roll = 0 perchÃ© la camera non ruota mai su se stessa
        // Quaternion.Euler(pitch, yaw, roll) = Quaternion.Euler(_currentY, _currentX, 0)
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // STEP 2: CALCOLA LA POSIZIONE TEORICA (SENZA COLLISIONI)
        // Moltiplica il quaternione per un vettore "avanti" negativo (reverse Z)
        // Questo ritorna un vettore che punta nella direzione opposta allo sguardo della camera
        // Se la camera guarda "forward", questo vettore punta indietro (reverse)
        Vector3 direction = rotation * new Vector3(0, 0, -_distance);
        // La posizione desiderata è il target + il vettore direction
        Vector3 desiredPosition = _target.position + direction;

        // STEP 3: GESTIONE COLLISIONI CON RAYCAST
        // Controlla se c'Ã¨ qualcosa tra il target e la posizione desiderata
        float finalDistance = CalculateFinalDistance(_target.position, desiredPosition);

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
        //   - velocity: ref parameter che traccia la velocitÃ  (SmoothDamp la modifica internamente)
        //   - smoothTime: tempo approssimativo per raggiungere il target (0.12s)
        // Questo Ã¨ meglio di Lerp perchÃ© evita overshooting e jitter
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            finalPosition, 
            ref _currentVelocity, 
            _positionSmoothTime
        );
        
        // PUNTA LA CAMERA VERSO IL TARGET
        // LookAt fa ruotare la camera per guardare un punto specifico
        // Vector3.up * 1.5f: solleva il punto di sguardo di 1.5 unitÃ 
        // (rende il frame piÃ¹ naturale, guardando leggermente in basso verso il player)
        transform.LookAt(_target.position + Vector3.up * 1.5f);
    }

    /// <summary>
    /// Calcola la distanza finale della camera, applicando collisioni se necessario.
    /// Metodo separato per facilitare il testing della logica di collisione.
    /// </summary>
    private float CalculateFinalDistance(Vector3 startPos, Vector3 endPos)
    {
        // Se le collisioni sono disabilitate, usa sempre la distanza massima desiderata
        if (!_enableCollision)
        {
            return _distance;
        }

        // Se le collisioni sono abilitate, controlla se c'è un ostacolo
        return CheckCameraCollision(startPos, endPos);
    }

    /// <summary>
    /// Controlla se la camera collide con qualcosa usando raycast.
    /// Ritorna la distanza sicura dalla camera al target (ridotta se c'Ã¨ un ostacolo).
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
        //   - _collisionLayers: layer mask per filtrare cosa puÃ² collidere
        if (Physics.Raycast(startPos, endPos - startPos, out hit, _distance, _collisionLayers))
        {
            // Se colpiamo un ostacolo, accorcia la distanza della camera
            // hit.distance: distanza dal startPos al punto di collisione
            // - _collisionPadding: spazio cuscinetto per non toccare il collider
            // Clamp: assicura che la distanza rimanga tra 0.5f e _distance
            // (evita che la camera vada troppo vicino o troppo lontano)
            return Mathf.Clamp(hit.distance - _collisionPadding, 0.5f, _distance);
        }
        // Se non c'Ã¨ collisione, usa la distanza massima desiderata
        return _distance;
    }
}