using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 7f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 6f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    
    [Header("Interaction Settings")]
    [SerializeField] private float _pushForce = 2f;
    [SerializeField] private float _recoilDuration = 0.65f;
    [SerializeField] private int _obstacleDamage = 7; // Danno inflitto dagli ostacoli

    [Header("References")]
    private Rigidbody _rb;
    private PlayerHealth _health; // Cache per lo script della salute

    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    private bool _isGrounded;
    private float _recoilTimer = 0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _health = GetComponent<PlayerHealth>(); // Recuperiamo la salute all'inizio

        if(_rb.freezeRotation != true)
        {
            _rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (_recoilTimer > 0)
        {
            _recoilTimer -= Time.deltaTime;
        }

        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        
        if(Input.GetButtonDown("Jump"))
        {
            _jumpInput = true;
        }
        
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    void FixedUpdate()
    {
        MovePlayer();
        HandleJump();
    }

    void MovePlayer()
    {
        if (_recoilTimer > 0) return;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraRight * _horizontalInput + cameraForward * _verticalInput;
        moveDirection.Normalize();
        
        _rb.velocity = new Vector3(
            moveDirection.x * _moveSpeed,
            _rb.velocity.y,
            moveDirection.z * _moveSpeed
        );
    }
    
    void HandleJump()
    {
        if(_jumpInput && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
        _jumpInput = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Controlliamo se è un muro o un ostacolo
        bool isBoundary = collision.gameObject.CompareTag("Boundaries");
        bool isObstacle = collision.gameObject.CompareTag("Obstacle");

        if (isBoundary || isObstacle)
        {
            // 1. Calcolo direzione di spinta (Normale del punto di contatto)
            Vector3 pushDirection = collision.contacts[0].normal;
            pushDirection.y = 0;
            pushDirection.Normalize();

            // 2. Applichiamo la fisica della spinta
            _rb.velocity = Vector3.zero;
            _rb.AddForce(pushDirection * _pushForce, ForceMode.Impulse);

            // 3. Blocchiamo i controlli (Recoil)
            _recoilTimer = _recoilDuration;

            // 4. Logica specifica per l'Ostacolo: DANNO
            if (isObstacle)
            {
                if (_health != null)
                {
                    _health.TakeDamage(_obstacleDamage);
                }
                Debug.Log($"Colpito Ostacolo! Danno ricevuto: {_obstacleDamage}");
            }
            else
            {
                Debug.Log("Colpito Muro di confine!");
            }
        }
    }
}