using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _partToRotate;
    [SerializeField] private Transform _firePoint;     // NUOVO: Da dove escono i colpi
    [SerializeField] private GameObject _bulletPrefab; // NUOVO: Cosa spariamo

    [Header("Settings")]
    [SerializeField] private float _rotationSpeed = 5f;
    [Range(-180f, 180f)]
    [SerializeField] private float _modelCorrection = 0f;

    [Header("Combat Settings")]
    [SerializeField] private float _fireRate = 1f; // Colpi al secondo

    // Variabili interne
    private Transform _playerTransform;
    private bool _isPlayerInRange = false; // NUOVO: Stato di attivazione
    private float _fireCountdown = 0f;     // NUOVO: Timer per il rateo di fuoco

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;

        if (_partToRotate == null) _partToRotate = transform;
    }

    void Update()
    {
        // Se il player non esiste, esci
        if (_playerTransform == null) return;

        // Eseguiamo la logica SOLO se il player è nel raggio (Trigger)
        if (_isPlayerInRange)
        {
            // 1. Inseguimento
            TrackPlayer();

            // 2. Gestione Sparo
            if (_fireCountdown <= 0f)
            {
                Shoot();
                _fireCountdown = 1f / _fireRate; // Resetta timer (es. 1/2 = 0.5s)
            }

            // Diminuisce il timer ogni frame
            _fireCountdown -= Time.deltaTime;
        }
    }

    void TrackPlayer()
    {
        Vector3 direction = _playerTransform.position - _partToRotate.position;
        direction.y = 0;

        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Applichiamo la tua correzione che hai trovato (es. 90)
        Quaternion correctedTarget = lookRotation * Quaternion.Euler(0f, _modelCorrection, 0f);

        _partToRotate.rotation = Quaternion.Lerp(
            _partToRotate.rotation,
            correctedTarget,
            Time.deltaTime * _rotationSpeed
        );
    }

    void Shoot()
    {
        if (_bulletPrefab != null && _firePoint != null)
        {
            // 1. Creiamo il proiettile
            GameObject bulletObj = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);

            // 2. FIX: Forziamo il proiettile a guardare il player istantaneamente
            // Questo sovrascrive qualsiasi errore di rotazione del FirePoint
            if (_playerTransform != null)
            {
                bulletObj.transform.LookAt(_playerTransform);
            }
        }
        else
        {
            Debug.LogError("Manca BulletPrefab o FirePoint sulla torretta!");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // LOGICA TRIGGER (Il sensore della torretta)
    // ════════════════════════════════════════════════════════════════

    void OnTriggerEnter(Collider other)
    {
        // Quando il player entra nella sfera invisibile
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Quando il player esce dalla sfera
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }

    // Debug Visivo: Disegna la sfera nell'editor per vedere il raggio
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        SphereCollider rangeCollider = GetComponent<SphereCollider>();
        if (rangeCollider != null)
        {
            Gizmos.DrawWireSphere(transform.position, rangeCollider.radius);
        }
    }
}