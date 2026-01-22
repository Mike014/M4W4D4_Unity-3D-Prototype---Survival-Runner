using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _partToRotate; 
    
    [Header("Settings")]
    [SerializeField] private float _rotationSpeed = 5f; 
    
    // Correzione dell'asse (90, -90, 180, etc.)
    [Range(-180f, 180f)]
    [SerializeField] private float _modelCorrection = 0f; 

    private Transform _playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;
        
        if (_partToRotate == null) _partToRotate = transform;
    }

    void Update()
    {
        if (_playerTransform == null) return;
        TrackPlayer();
    }

    void TrackPlayer()
    {
        // 1. Direzione verso il player
        Vector3 direction = _playerTransform.position - _partToRotate.position;
        direction.y = 0; // Blocchiamo l'altezza

        // 2. Calcoliamo la rotazione base (dove guarderebbe Unity di default)
        // Se la direzione è zero (player esattamente sopra la torretta), non ruotare
        if (direction == Vector3.zero) return; 
        
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // 3. APPLICHIAMO LA CORREZIONE QUI (Matematica dei Quaternioni)
        // Moltiplicare due quaternioni equivale a sommare le loro rotazioni.
        // Calcoliamo la rotazione FINALE corretta prima di muoverci.
        Quaternion correctedTarget = lookRotation * Quaternion.Euler(0f, _modelCorrection, 0f);

        // 4. Ora ci muoviamo fluidamente verso il target GIÀ corretto
        _partToRotate.rotation = Quaternion.Lerp(
            _partToRotate.rotation, 
            correctedTarget, 
            Time.deltaTime * _rotationSpeed
        );
    }
}