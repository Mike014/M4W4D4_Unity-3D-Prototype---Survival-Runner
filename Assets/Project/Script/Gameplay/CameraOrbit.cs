using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    
    [Header("Orbit Settings")]
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _minVerticalAngle = -40f;
    [SerializeField] private float _maxVerticalAngle = 80f;
    
    [Header("Smoothing")]
    // [SerializeField] private float _rotationSmoothSpeed = 10f; // Smooth solo per la rotazione
    
    // Variabili private per tracciare la rotazione
    private float _currentX = 0f;
    private float _currentY = 0f;
    
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void LateUpdate()
    {
        if (_target == null) return;
        
        RotateCamera();
        PositionCamera();
    }
    
    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        
        _currentX += mouseX;
        _currentY -= mouseY;
        
        _currentY = Mathf.Clamp(_currentY, _minVerticalAngle, _maxVerticalAngle);
    }
    
    void PositionCamera()
    {
        // Creiamo la rotazione
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // ════════════════════════════════════════════════════════════════
        // MODIFICA CHIAVE: Posizione DIRETTA senza Lerp
        // Il player è già interpolato dal Rigidbody, non serve double smoothing
        // ════════════════════════════════════════════════════════════════
        
        Vector3 negDistance = new Vector3(0, 0, -_distance);
        Vector3 desiredPosition = _target.position + rotation * negDistance;
        
        // NUOVA VERSIONE: Posizione diretta (no lerp sulla posizione)
        transform.position = desiredPosition;
        
        // VECCHIA VERSIONE (causava jitter con interpolation):
        // transform.position = Vector3.Lerp(
        //     transform.position, 
        //     desiredPosition, 
        //     _smoothSpeed * Time.deltaTime
        // );
        
        // La camera guarda sempre il target
        transform.LookAt(_target);
    }
}