using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _lifeTime = 3f; // Distrugge il proiettile dopo 3 secondi

    void Start()
    {
        // Distruzione automatica per pulire la scena
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        // Movimento in avanti costante
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Se colpiamo il Player
        if (other.CompareTag("Player"))
        {
            // Cerchiamo lo script della vita
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);
            }

            // Distruggiamo il proiettile all'impatto
            Destroy(gameObject);
        }
        // Opzionale: Distruggi se colpisce muri o pavimento
        else if (!other.CompareTag("Turret") && !other.CompareTag("Bullet")) 
        {
            Destroy(gameObject);
        }
    }
}
