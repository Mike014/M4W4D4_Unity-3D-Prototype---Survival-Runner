using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Vector3 _openOffset = new Vector3(0, 5, 0); // Dove si sposta
    [SerializeField] private float _openSpeed = 2f;
    private bool _isOpen = false;
    private Vector3 _targetPosition;

    void Start() => _targetPosition = transform.position;

    public void Open() 
    {
        _isOpen = true;
        _targetPosition = transform.position + _openOffset;
        Debug.Log("Porta Sbloccata!");
    }

    void Update()
    {
        if (_isOpen)
        {
            // Movimento fluido verso la posizione aperta
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _openSpeed);
        }
    }
}
