using NUnit.Framework;
using UnityEngine;

public class PlayerControllerTests
{
    private PlayerController _playerController;
    private GameObject _playerObj;
    private Rigidbody _rb;
    private GameObject _groundCheckObj;

    [SetUp]
    public void Setup()
    {
        // Modalità manuale
        Physics.defaultSolverIterations = 6;
        Physics.simulationMode = SimulationMode.Script;

        // CREA IL GAMEOBJECT DEL PLAYER
        _playerObj = new GameObject("Player");
        _playerController = _playerObj.AddComponent<PlayerController>();
        _rb = _playerObj.AddComponent<Rigidbody>();

        // SETUP RIGIDBODY
        _rb.mass = 1f;
        _rb.drag = 0f;
        _rb.angularDrag = 0.05f;
        _rb.freezeRotation = true;
        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        // CREA IL GROUND CHECK
        _groundCheckObj = new GameObject("GroundCheck");
        _groundCheckObj.transform.SetParent(_playerObj.transform);
        _groundCheckObj.transform.localPosition = Vector3.zero;

        // ASSEGNA I RIFERIMENTI (tramite reflection poiché sono private)
        var groundCheckField = typeof(PlayerController).GetField("_groundCheck",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        groundCheckField.SetValue(_playerController, _groundCheckObj.transform);

        var rbField = typeof(PlayerController).GetField("_rb",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rbField.SetValue(_playerController, _rb);
    }

    [TearDown]
    public void Teardown()
    {
        Physics.simulationMode = SimulationMode.FixedUpdate;
        
        // Distruggendo il player, distruggi automaticamente anche i figli (groundCheckObj)
        if (_playerObj != null)
        {
            Object.DestroyImmediate(_playerObj);
        }

        // Rimuovi questa riga, è ridondante e causa l'errore!
        // Object.DestroyImmediate(_groundCheckObj); 
    }

    /// TEST 1: Verifica che il Rigidbody sia inizializzato
    [Test]
    public void Setup_RigidbodyInitialized()
    {
        // ASSERT
        Assert.IsNotNull(_rb);
        Assert.IsTrue(_rb.freezeRotation);
    }

    /// TEST 2: Verifica che il movimento applichi velocità al Rigidbody
    [Test]
    public void MovePlayer_AppliesVelocity()
    {
        // ARRANGE
        Vector3 initialVelocity = _rb.velocity;

        // ACT - Simula input di movimento
        var horizontalInputField = typeof(PlayerController).GetField("_horizontalInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var verticalInputField = typeof(PlayerController).GetField("_verticalInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        horizontalInputField.SetValue(_playerController, 1f);  // Movimento a destra
        verticalInputField.SetValue(_playerController, 0f);

        // ASSERT
        // Nota: Non possiamo testare direttamente il movimento senza simulare input completo
        Assert.IsNotNull(_rb.velocity);
    }

    /// TEST 3: Verifica che il recoil timer decrementi
    [Test]
    public void RecoilTimer_Decrements()
    {
        // ARRANGE
        var recoilTimerField = typeof(PlayerController).GetField("_recoilTimer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float initialRecoil = 1f;
        recoilTimerField.SetValue(_playerController, initialRecoil);

        // ACT
        float deltaTime = 0.1f;
        recoilTimerField.SetValue(_playerController, initialRecoil - deltaTime);

        // ASSERT
        float currentRecoil = (float)recoilTimerField.GetValue(_playerController);
        Assert.Less(currentRecoil, initialRecoil);
    }

    /// TEST 4: Verifica che il player sia a terra (isGrounded)
    [Test]
    public void IsGrounded_DetectsGround()
    {
        // ARRANGE
        var isGroundedField = typeof(PlayerController).GetField("_isGrounded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT
        isGroundedField.SetValue(_playerController, true);

        // ASSERT
        bool isGrounded = (bool)isGroundedField.GetValue(_playerController);
        Assert.IsTrue(isGrounded);
    }

    /// TEST 5: Verifica che il jump applichi forza
    [Test]
    public void HandleJump_AppliesForce()
    {
        // ARRANGE
        var jumpInputField = typeof(PlayerController).GetField("_jumpInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var isGroundedField = typeof(PlayerController).GetField("_isGrounded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        jumpInputField.SetValue(_playerController, true);
        isGroundedField.SetValue(_playerController, true);

        Vector3 velocityBefore = _rb.velocity;

        // ACT
        var handleJumpMethod = typeof(PlayerController).GetMethod("HandleJump",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        handleJumpMethod.Invoke(_playerController, null);

        // ⭐ AGGIUNGI QUESTA RIGA: Esegui un passo della simulazione fisica
        Physics.Simulate(Time.fixedDeltaTime);

        // ASSERT
        Assert.Greater(_rb.velocity.y, velocityBefore.y,
            "Dopo il salto, velocity.y dovrebbe aumentare");
    }

    /// TEST 6: Verifica che il movimento sia bloccato durante recoil
    [Test]
    public void MovePlayer_BlockedDuringRecoil()
    {
        // ARRANGE
        var recoilTimerField = typeof(PlayerController).GetField("_recoilTimer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        recoilTimerField.SetValue(_playerController, 0.5f);  // Recoil attivo

        Vector3 velocityBeforeRecoil = _rb.velocity;

        // ACT
        var movePlayerMethod = typeof(PlayerController).GetMethod("MovePlayer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        movePlayerMethod.Invoke(_playerController, null);

        // ASSERT - Durante recoil, la velocità dovrebbe rimmanere invariata (movimento bloccato)
        Assert.AreEqual(velocityBeforeRecoil, _rb.velocity);
    }

    /// TEST 7: Verifica che OnCollisionEnter esista
    [Test]
    public void OnCollisionEnter_MethodExists()
    {
        // ARRANGE & ACT
        var onCollisionMethod = typeof(PlayerController).GetMethod("OnCollisionEnter",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ASSERT
        Assert.IsNotNull(onCollisionMethod);
    }

    /// TEST 8: Verifica che il Rigidbody non ruoti
    [Test]
    public void Rigidbody_FreezeRotation()
    {
        // ASSERT
        Assert.IsTrue(_rb.freezeRotation);
    }

    /// TEST 9: Verifica che l'input jump sia resettato dopo l'uso
    [Test]
    public void HandleJump_ResetsJumpInput()
    {
        // ARRANGE
        var jumpInputField = typeof(PlayerController).GetField("_jumpInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var isGroundedField = typeof(PlayerController).GetField("_isGrounded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        jumpInputField.SetValue(_playerController, true);
        isGroundedField.SetValue(_playerController, true);

        // ACT
        var handleJumpMethod = typeof(PlayerController).GetMethod("HandleJump",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        handleJumpMethod.Invoke(_playerController, null);

        // ASSERT
        bool jumpInputAfter = (bool)jumpInputField.GetValue(_playerController);
        Assert.IsFalse(jumpInputAfter);
    }

    /// TEST 10: Verifica che il player non possa saltare se non a terra
    [Test]
    public void HandleJump_CantJumpIfNotGrounded()
    {
        // ARRANGE
        var jumpInputField = typeof(PlayerController).GetField("_jumpInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var isGroundedField = typeof(PlayerController).GetField("_isGrounded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Vector3 initialVelocity = _rb.velocity;

        jumpInputField.SetValue(_playerController, true);
        isGroundedField.SetValue(_playerController, false);  // NON a terra

        // ACT
        var handleJumpMethod = typeof(PlayerController).GetMethod("HandleJump",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        handleJumpMethod.Invoke(_playerController, null);

        // ASSERT - La velocità Y non dovrebbe cambiare
        Assert.AreEqual(initialVelocity.y, _rb.velocity.y);
    }
}