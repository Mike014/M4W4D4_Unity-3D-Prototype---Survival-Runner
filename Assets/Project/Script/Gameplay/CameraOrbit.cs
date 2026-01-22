using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target; // Il player da seguire
    
    [Header("Orbit Settings")]
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private float _distance = 5f; // Distanza dalla camera al player
    [SerializeField] private float _minVerticalAngle = -40f; // Limite angolo verso il basso
    [SerializeField] private float _maxVerticalAngle = 80f;  // Limite angolo verso l'alto
    
    [Header("Smoothing")]
    [SerializeField] private float _smoothSpeed = 10f; // Quanto smooth è il seguimento
    
    // Variabili private per tracciare la rotazione
    private float _currentX = 0f; // Rotazione orizzontale (yaw)
    private float _currentY = 0f; // Rotazione verticale (pitch)
    
    void Start()
    {
        // IMPORTANTE: Nascondiamo e blocchiamo il cursore per controllo in prima persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Inizializziamo gli angoli con la rotazione corrente della camera
        Vector3 angles = transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
    }
    
    void Update()
    {
        // ESC per sbloccare il cursore (utile per testing)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Click per ri-bloccare il cursore
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void LateUpdate()
    {
        // IMPORTANTE: Usiamo LateUpdate per assicurarci che il player si sia già mosso
        // Altrimenti la camera "anticiperebbe" il movimento causando jitter
        
        if (_target == null) return; // Safety check
        
        RotateCamera();
        PositionCamera();
    }
    
    void RotateCamera()
    {
        // Leggiamo il movimento del mouse
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        
        // Aggiorniamo gli angoli di rotazione
        _currentX += mouseX;
        
        // NOTA: Sottraiamo invece di sommare perché il mouse Y è "invertito"
        _currentY -= mouseY;
        
        // Clampiamo l'angolo verticale per evitare che la camera vada sottosopra
        _currentY = Mathf.Clamp(_currentY, _minVerticalAngle, _maxVerticalAngle);
    }
    
    void PositionCamera()
    {
        // Creiamo un Quaternion (rotazione) dagli angoli calcolati
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // Calcoliamo la posizione desiderata della camera
        Vector3 negDistance = new Vector3(0, 0, -_distance);
        Vector3 desiredPosition = _target.position + rotation * negDistance;
        
        // Smooth seguimento: interpoliamo tra posizione attuale e desiderata
        transform.position = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            _smoothSpeed * Time.deltaTime
        );
        
        // Facciamo guardare la camera verso il target
        transform.LookAt(_target);
    }
}