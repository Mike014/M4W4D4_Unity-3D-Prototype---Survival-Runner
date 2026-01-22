using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _scoreValue = 1;      
    [SerializeField] private float _timeBonus = 5f;    // Controlla che la 's' ci sia!
    [SerializeField] private bool _isSpecial = false;  

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // NOTA: Usa 'Instance' (con la C), non 'Instantiate' (con la T)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin(_scoreValue, _timeBonus);
            }
            
            Destroy(gameObject);
        }
    }
}
