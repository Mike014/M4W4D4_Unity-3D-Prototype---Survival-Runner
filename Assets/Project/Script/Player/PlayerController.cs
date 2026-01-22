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
    
    [Header("References")]
    private Rigidbody _rb;

    // Variabili per memorizzare l'input
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    
    // Variabile per tracciare se siamo a terra
    private bool _isGrounded;

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
        // ════════════════════════════════════════════════════════════════
        // INPUT - Leggiamo in Update per massima responsività
        // ════════════════════════════════════════════════════════════════
        
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
        
        // Physics.CheckSphere crea una sfera invisibile nella posizione specificata
        // Ritorna true se la sfera tocca qualsiasi collider nel layer specificato
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
        // ════════════════════════════════════════════════════════════════
        // LOGICA DEL SALTO
        // ════════════════════════════════════════════════════════════════
        
        // Possiamo saltare SOLO se:
        // 1. Abbiamo premuto spazio (_jumpInput = true)
        // 2. Siamo a terra (_isGrounded = true)
        if(_jumpInput && _isGrounded)
        {
            // AddForce applica una forza istantanea
            // ForceMode.Impulse = applica la forza tenendo conto della massa
            // Direzione: Vector3.up = (0, 1, 0)
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
        
        // IMPORTANTE: Resettiamo _jumpInput dopo averlo processato
        // Altrimenti il salto verrebbe applicato ogni FixedUpdate!
        _jumpInput = false;
    }
    
    // ════════════════════════════════════════════════════════════════
    // VISUALIZZAZIONE DEBUG - Vedere la sfera di ground check
    // ════════════════════════════════════════════════════════════════
    
    void OnDrawGizmosSelected()
    {
        // OnDrawGizmosSelected viene chiamato dall'editor quando selezioni l'oggetto
        // Serve per visualizzare elementi di debug nella Scene view
        
        if(_groundCheck == null) return;
        
        // Colore della sfera: verde se a terra, rosso se in aria
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        
        // Disegna una sfera wireframe (solo contorno) nella posizione del ground check
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}