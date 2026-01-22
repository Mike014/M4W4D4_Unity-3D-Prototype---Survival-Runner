using UnityEngine;

public class HealthTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth; // Trascina qui il Player

    [Header("Test Settings")]
    [SerializeField] private int _damageAmount = 10;
    [SerializeField] private int _healAmount = 10;

    void Update()
    {
        // Tasto Sinistro (0) -> Danno
        if (Input.GetMouseButtonDown(0))
        {
            if (_playerHealth != null)
            {
                _playerHealth.TakeDamage(_damageAmount);
                // Debug.Log("Test Click: Tentativo di danno.");
            }
            else
            {
                Debug.LogWarning("HealthTester: Manca il riferimento a PlayerHealth!");
            }
        }

        // Tasto Destro (1) -> Cura (utile per resettare)
        if (Input.GetMouseButtonDown(1))
        {
            if (_playerHealth != null)
            {
                _playerHealth.Heal(_healAmount);
            }
        }
    }
}