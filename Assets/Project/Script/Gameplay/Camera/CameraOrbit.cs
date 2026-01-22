using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    
    [Header("Orbit Settings")]
    [SerializeField] private float _mouseSensitivity = 2f; 
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _minVerticalAngle = -20f;
    [SerializeField] private float _maxVerticalAngle = 70f;
    
    [Header("Collision & Smoothing")]
    [SerializeField] private bool _enableCollision = true;
    [SerializeField] private LayerMask _collisionLayers; 
    [SerializeField] private float _collisionPadding = 0.2f; // Distanza dai muri
    [SerializeField] private float _positionSmoothTime = 0.12f; // Tempo di smorzamento

    private float _currentX = 0f;
    private float _currentY = 0f;
    private Vector3 _currentVelocity = Vector3.zero; // Necessaria per SmoothDamp

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Vector3 angles = transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
    }

    void Update()
    {
        // Gestione mouse input spostata qui per massima reattività
        HandleInput();
        
        // Sblocco cursore
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        if (_target == null) return;
        
        UpdateCameraTransform();
    }

    private void HandleInput()
    {
        // Rimosso Time.deltaTime: l'input del mouse è già un delta frame-based
        _currentX += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _currentY -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        
        _currentY = Mathf.Clamp(_currentY, _minVerticalAngle, _maxVerticalAngle);
    }

    private void UpdateCameraTransform()
    {
        // 1. Calcolo rotazione desiderata
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // 2. Calcolo posizione teorica (distanza massima)
        Vector3 direction = rotation * new Vector3(0, 0, -_distance);
        Vector3 desiredPosition = _target.position + direction;

        // 3. Gestione Collisioni (Raycast)
        float finalDistance = _distance;
        if (_enableCollision)
        {
            finalDistance = CheckCameraCollision(_target.position, desiredPosition);
        }

        // 4. Calcolo posizione finale corretta per le collisioni
        Vector3 finalPosition = _target.position + (direction.normalized * finalDistance);

        // 5. Smoothing con SmoothDamp invece di Lerp
        // SmoothDamp è più efficiente e previene il jitter con i Rigidbody
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _currentVelocity, _positionSmoothTime);
        
        // La camera guarda sempre il target
        transform.LookAt(_target.position + Vector3.up * 1.5f); // Alziamo il punto di sguardo leggermente
    }

    private float CheckCameraCollision(Vector3 startPos, Vector3 endPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(startPos, endPos - startPos, out hit, _distance, _collisionLayers))
        {
            // Se colpiamo qualcosa, accorciamo la distanza della camera
            return Mathf.Clamp(hit.distance - _collisionPadding, 0.5f, _distance);
        }
        return _distance;
    }
}