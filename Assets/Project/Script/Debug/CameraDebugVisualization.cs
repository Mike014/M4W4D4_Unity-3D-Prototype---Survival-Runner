using UnityEngine;

/// <summary>
/// Script di DEBUG per visualizzare la posizione e la direzione della camera.
/// Utile durante lo sviluppo per capire dove sta puntando la camera all'avvio del gioco.
/// Disabilita questo script in produzione (non è necessario nella build finale).
/// </summary>
public class CameraDebugVisualization : MonoBehaviour
{
    [Header("Debug Settings")]
    // Colore della linea che mostra la direzione forward della camera
    [SerializeField] private Color _forwardLineColor = Color.blue;
    // Colore della linea che mostra il vettore up (alto della camera)
    [SerializeField] private Color _upLineColor = Color.green;
    // Colore della linea che mostra il vettore right (destra della camera)
    [SerializeField] private Color _rightLineColor = Color.red;
    // Lunghezza delle linee di debug (quanto lontano disegnare le linee)
    [SerializeField] private float _lineLength = 10f;
    // Se true, disegna le linee in ogni frame (costante visività)
    [SerializeField] private bool _drawDebugLines = true;
    // Se true, stampa nella console la posizione e rotazione della camera
    [SerializeField] private bool _logToConsole = true;

    // Riferimento alla camera principale del gioco
    private Camera _mainCamera;

    /// <summary>
    /// Inizializza lo script di debug all'avvio della scena.
    /// Recupera il riferimento alla camera principale e stampa le informazioni iniziali.
    /// </summary>
    void Start()
    {
        // RECUPERA LA CAMERA PRINCIPALE
        // Camera.main è un shortcut per trovare la camera con il tag "MainCamera"
        _mainCamera = Camera.main;

        // VALIDAZIONE
        if (_mainCamera == null)
        {
            Debug.LogError("[CameraDebug] Camera principale non trovata! Assicurati che ci sia una camera con il tag 'MainCamera'");
            return;
        }

        // DEBUG: Stampa le informazioni iniziali della camera
        if (_logToConsole)
        {
            PrintCameraInfo();
        }
    }

    /// <summary>
    /// Aggiorna il debug ogni frame.
    /// Disegna le linee di debug (gizmo) che mostrano la posizione e orientamento della camera.
    /// </summary>
    void Update()
    {
        // Se è disabilitato il disegno delle linee, non fare nulla
        if (!_drawDebugLines || _mainCamera == null)
            return;

        // DISEGNA LE LINEE DI DEBUG
        // Linea BLU: Direzione forward (dove sta guardando la camera)
        // Questa è la direzione principale di visione
        Debug.DrawLine(
            _mainCamera.transform.position,
            _mainCamera.transform.position + _mainCamera.transform.forward * _lineLength,
            _forwardLineColor
        );

        // Linea VERDE: Direzione up (alto della camera)
        // Mostra quale direzione è "su" nella visione della camera
        Debug.DrawLine(
            _mainCamera.transform.position,
            _mainCamera.transform.position + _mainCamera.transform.up * _lineLength,
            _upLineColor
        );

        // Linea ROSSA: Direzione right (destra della camera)
        // Mostra la direzione "destra" nella visione della camera
        Debug.DrawLine(
            _mainCamera.transform.position,
            _mainCamera.transform.position + _mainCamera.transform.right * _lineLength,
            _rightLineColor
        );
    }

    /// <summary>
    /// Stampa nella console le informazioni dettagliate sulla posizione e rotazione della camera.
    /// Viene chiamato all'avvio e può essere chiamato manualmente per aggiornare le informazioni.
    /// </summary>
    private void PrintCameraInfo()
    {
        // INFORMAZIONI DI POSIZIONE
        Vector3 camPos = _mainCamera.transform.position;
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"[CAMERA DEBUG] POSIZIONE INIZIALE");
        Debug.Log($"  X: {camPos.x:F2}");
        Debug.Log($"  Y: {camPos.y:F2}");
        Debug.Log($"  Z: {camPos.z:F2}");
        Debug.Log($"  Posizione completa: {camPos}");

        // INFORMAZIONI DI ROTAZIONE
        Vector3 camRotation = _mainCamera.transform.eulerAngles;
        Debug.Log($"[CAMERA DEBUG] ROTAZIONE INIZIALE (Euler Angles)");
        Debug.Log($"  X (Pitch): {camRotation.x:F2}° (rotazione su/giù)");
        Debug.Log($"  Y (Yaw): {camRotation.y:F2}° (rotazione sinistra/destra)");
        Debug.Log($"  Z (Roll): {camRotation.z:F2}° (rotazione su se stessa)");

        // DIREZIONI DELLA CAMERA
        Debug.Log($"[CAMERA DEBUG] DIREZIONI DI VISIONE");
        Debug.Log($"  Forward (BLU): {_mainCamera.transform.forward}");
        Debug.Log($"  Up (VERDE): {_mainCamera.transform.up}");
        Debug.Log($"  Right (ROSSA): {_mainCamera.transform.right}");

        // INFORMAZIONI DEL TARGET (se orbita intorno a qualcosa)
        Debug.Log($"[CAMERA DEBUG] INFORMAZIONI AGGIUNTIVE");
        Debug.Log($"  FOV (Field of View): {_mainCamera.fieldOfView}°");
        Debug.Log($"  Near Clip: {_mainCamera.nearClipPlane}");
        Debug.Log($"  Far Clip: {_mainCamera.farClipPlane}");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    /// <summary>
    /// Metodo pubblico che può essere chiamato da altri script o dalla console
    /// per aggiornare le informazioni di debug della camera.
    /// Utile se vuoi stampare le informazioni nuovamente durante il gameplay.
    /// </summary>
    public void RefreshDebugInfo()
    {
        if (_mainCamera == null)
        {
            Debug.LogError("[CameraDebug] Camera principale non trovata!");
            return;
        }

        PrintCameraInfo();
    }

    /// <summary>
    /// Disegna un punto di debug nella posizione specificata.
    /// Utile per visualizzare punti importanti nello spazio (target, waypoint, ecc).
    /// </summary>
    /// <param name="position">Posizione dove disegnare il punto</param>
    /// <param name="color">Colore del punto</param>
    /// <param name="size">Grandezza del punto</param>
    public void DrawDebugPoint(Vector3 position, Color color, float size = 0.5f)
    {
        // Disegna una piccola sfera intorno al punto
        Debug.DrawLine(position + Vector3.up * size, position - Vector3.up * size, color);
        Debug.DrawLine(position + Vector3.right * size, position - Vector3.right * size, color);
        Debug.DrawLine(position + Vector3.forward * size, position - Vector3.forward * size, color);
    }
}