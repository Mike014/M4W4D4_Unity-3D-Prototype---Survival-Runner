using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Variabili private
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
        // Movimento in FixedUpdate perhè usiamo la fisica
        // ----
        MovePlayer();
    }

    void MovePlayer()
    {
        // Otteniamo la direzione in cui la camera sta guardando
        // ma IGNORIAMO l'inclinazione verticale (y = 0)
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Proiettiamo sul piano orizzontale
        cameraForward.y = 0;
        cameraRight.y = 0;


        // Normalizziamo per sicurezza
        cameraForward.Normalize();
        cameraRight.Normalize();

        // vettore di movimento basato sull'input
        Vector3 moveDirection = cameraRight * _horizontalInput + cameraForward *_verticalInput;

        // Normalizziamo il vettore per evitare movimento più veloce in diagonale
        moveDirection.Normalize();

        _rb.velocity = new Vector3(
            moveDirection.x * _moveSpeed, _rb.velocity.y, moveDirection.z * _moveSpeed
        );
        
    }
}
