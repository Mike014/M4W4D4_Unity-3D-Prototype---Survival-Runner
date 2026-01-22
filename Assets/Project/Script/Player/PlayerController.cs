using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private Transform _groundCheck; // Posizione del check sotto il player
    [SerializeField] private float _groundCheckRadius = 0.2f; // Raggio della sfera di check
    [SerializeField] private LayerMask _groundLayer; // Layer del terreno
    
    [Header("Interaction Settings")]
    [SerializeField] private float _pushForce = 10f; // Forza della spinta indietro
    [SerializeField] private float _recoilDuration = 0.65f; // Tempo in cui i controlli sono bloccati dopo l'urto

    [Header("References")]
    private Rigidbody _rb;

    // Variabili per memorizzare l'input
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    
    // Variabile per tracciare se siamo a terra
    private bool _isGrounded;

    // Variabile per gestire il blocco movimento dopo l'urto
    private float _recoilTimer = 0f;

    void Start()
    {
        // Riferimento rigidbody
        _rb = GetComponent<Rigidbody>();

        // Freeze rotation checker
        if(_rb.freezeRotation != true)
        {
            _rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // Gestione timer del rinculo
        if (_recoilTimer > 0)
        {
            _recoilTimer -= Time.deltaTime;
        }

        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        
        // GetButtonDown = true SOLO nel frame in cui premi
        // Questo evita "salti continui" tenendo premuto spazio
        if(Input.GetButtonDown("Jump"))
        {
            _jumpInput = true;
        }
        
        // ════════════════════════════════════════════════════════════════
        // GROUND CHECK - Verifichiamo se siamo a terra
        // ════════════════════════════════════════════════════════════════
        
        _isGrounded = Physics.CheckSphere(
            _groundCheck.position,      // Posizione del check
            _groundCheckRadius,          // Raggio della sfera
            _groundLayer                 // Layer da controllare
        );
    }

    void FixedUpdate()
    {
        // Movimento in FixedUpdate perché usiamo la fisica
        MovePlayer();
        HandleJump();
    }

    void MovePlayer()
    {
        // IMPORTANTE: Se siamo sotto effetto del rinculo (recoil),
        // NON applichiamo il movimento volontario, lasciamo agire la fisica della spinta.
        if (_recoilTimer > 0) return;

        // Otteniamo la direzione in cui la camera sta guardando
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Proiettiamo sul piano orizzontale
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Normalizziamo per sicurezza
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Vettore di movimento basato sull'input
        Vector3 moveDirection = cameraRight * _horizontalInput + cameraForward * _verticalInput;
        moveDirection.Normalize();
        
        // Impostiamo la velocità orizzontale, mantenendo quella verticale
        _rb.velocity = new Vector3(
            moveDirection.x * _moveSpeed,
            _rb.velocity.y,  // Manteniamo velocità verticale (gravità/salto)
            moveDirection.z * _moveSpeed
        );
    }
    
    void HandleJump()
    {
        // Possiamo saltare SOLO se:
        // 1. Abbiamo premuto spazio (_jumpInput = true)
        // 2. Siamo a terra (_isGrounded = true)
        if(_jumpInput && _isGrounded)
        {
            // AddForce applica una forza istantanea
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
        
        // IMPORTANTE: Resettiamo _jumpInput dopo averlo processato
        _jumpInput = false;
    }

    // ════════════════════════════════════════════════════════════════
    // INTERAZIONE CON I MURI (BOUNDARIES)
    // ════════════════════════════════════════════════════════════════
    private void OnCollisionEnter(Collision collision)
    {
        // Controlliamo se l'oggetto toccato ha il tag "Boundaries"
        if (collision.gameObject.CompareTag("Boundaries"))
        {
            // Calcoliamo la direzione opposta all'impatto.
            // collision.contacts[0].normal ci dà il vettore perpendicolare alla superficie colpita (che punta verso fuori)
            Vector3 pushDirection = collision.contacts[0].normal;

            // Appiattiamo la spinta sull'asse Y per evitare che il player voli via se colpisce un muro storto
            pushDirection.y = 0;
            pushDirection.Normalize();

            // Resettiamo la velocità attuale per dare un impatto pulito
            _rb.velocity = Vector3.zero;

            // Applichiamo la forza impulsiva indietro
            _rb.AddForce(pushDirection * _pushForce, ForceMode.Impulse);

            // Attiviamo il timer di rinculo per bloccare l'input per un breve istante
            _recoilTimer = _recoilDuration;
            
            Debug.Log($"Colpito muro! Spinta in direzione: {pushDirection}");
        }
    }
}