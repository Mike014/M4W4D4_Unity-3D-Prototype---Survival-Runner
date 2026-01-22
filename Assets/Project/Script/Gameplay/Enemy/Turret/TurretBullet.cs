using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _lifeTime = 1.5f;

    void Start()
    {
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Ignoriamo i Trigger (zone di attivazione, sensori, ecc.)
        if (other.isTrigger) return;

        // DEBUG: Vediamo nella console cosa abbiamo colpito
        Debug.Log($"Proiettile ha colpito: {other.name} (Tag: {other.tag})");

        // 2. Cerchiamo la vita SULL'OGGETTO O SUI GENITORI
        // GetComponentInParent è più sicuro perché trova lo script anche se colpiamo un piede o un braccio
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        // Se abbiamo trovato la vita (quindi è il Player)
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_damage);
            Destroy(gameObject); // Distruggiamo il proiettile dopo aver fatto danno
        }
        // Se NON è il player, NON è la torretta e NON è un altro proiettile... distruggiamo (muri, pavimento)
        else if (!other.CompareTag("Turret") && !other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}