using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("References")]
    private Rigidbody _rb;

    // Variabili per memorizzare l'input
    private float _horizontalInput;
    private float _verticalInput;

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
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        // Movimento in FixedUpdate perché usiamo la fisica
        MovePlayer();
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

        // ════════════════════════════════════════════════════════════════
        // SOLUZIONE AL JITTER: Usa velocity invece di MovePosition
        // con Interpolation attiva!
        // ════════════════════════════════════════════════════════════════
        
        // Impostiamo la velocità orizzontale, mantenendo quella verticale
        _rb.velocity = new Vector3(
            moveDirection.x * _moveSpeed,
            _rb.velocity.y,  // Manteniamo velocità verticale (gravità/salto)
            moveDirection.z * _moveSpeed
        );
    }
}