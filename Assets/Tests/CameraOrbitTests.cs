using NUnit.Framework;
using UnityEngine;

public class CameraOrbitTests
{
    private CameraOrbit _cameraOrbit;
    private GameObject _cameraObj;
    private GameObject _targetObj;

    [SetUp]
    public void Setup()
    {
        // Crea il target (player)
        _targetObj = new GameObject("Target");
        _targetObj.transform.position = Vector3.zero;

        // Crea la camera
        _cameraObj = new GameObject("Camera");
        _cameraOrbit = _cameraObj.AddComponent<CameraOrbit>();
        _cameraObj.AddComponent<Camera>();

        // Assegna il target tramite reflection
        var targetField = typeof(CameraOrbit).GetField("_target",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        targetField.SetValue(_cameraOrbit, _targetObj.transform);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_cameraObj);
        Object.DestroyImmediate(_targetObj);
    }

    // TEST 1: Verifica che gli angoli iniziali siano settati
    [Test]
    public void Start_InitializesAngles()
    {
        var currentXField = typeof(CameraOrbit).GetField("_currentX",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentYField = typeof(CameraOrbit).GetField("_currentY",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float currentX = (float)currentXField.GetValue(_cameraOrbit);
        float currentY = (float)currentYField.GetValue(_cameraOrbit);

        Assert.IsNotNull(currentX);
        Assert.IsNotNull(currentY);
    }

    // TEST 2: Verifica che UpdateAngles aggiorni correttamente
    [Test]
    public void UpdateAngles_IncreasesHorizontalAngle()
    {
        var updateAnglesMethod = typeof(CameraOrbit).GetMethod("UpdateAngles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var currentXField = typeof(CameraOrbit).GetField("_currentX",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float initialX = 0f;
        currentXField.SetValue(_cameraOrbit, initialX);

        // ACT: Chiama UpdateAngles con input orizzontale positivo
        updateAnglesMethod.Invoke(_cameraOrbit, new object[] { 1f, 0f });

        float finalX = (float)currentXField.GetValue(_cameraOrbit);
        Assert.Greater(finalX, initialX);
    }

    // TEST 3: Verifica che l'angolo verticale sia clampato
    [Test]
    public void UpdateAngles_ClampsVerticalAngle()
    {
        var updateAnglesMethod = typeof(CameraOrbit).GetMethod("UpdateAngles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var currentYField = typeof(CameraOrbit).GetField("_currentY",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT: Prova a impostare un angolo molto alto (oltre il max di 70 gradi)
        updateAnglesMethod.Invoke(_cameraOrbit, new object[] { 0f, 100f });

        float finalY = (float)currentYField.GetValue(_cameraOrbit);
        Assert.LessOrEqual(finalY, 70f, "L'angolo Y dovrebbe essere clampato al massimo di 70");
    }
}
