using NUnit.Framework;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// TURRET CONTROLLER TESTS
// ═══════════════════════════════════════════════════════════════════════════════

public class TurretControllerTests
{
    private TurretController _turretController;
    private GameObject _turretObj;
    private GameObject _partToRotateObj;

    [SetUp]
    public void Setup()
    {
        // CREA IL GAMEOBJECT DELLA TORRETTA
        _turretObj = new GameObject("Turret");
        _turretController = _turretObj.AddComponent<TurretController>();

        // CREA IL GAMEOBJECT CHE RUOTA (testa della torretta)
        _partToRotateObj = new GameObject("PartToRotate");
        _partToRotateObj.transform.SetParent(_turretObj.transform);
        _partToRotateObj.transform.localPosition = Vector3.zero;

        // ASSEGNA I RIFERIMENTI VIA REFLECTION
        var partToRotateField = typeof(TurretController).GetField("_partToRotate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        partToRotateField.SetValue(_turretController, _partToRotateObj.transform);
    }

    [TearDown]
    public void Teardown()
    {
        if (_turretObj != null)
        {
            Object.DestroyImmediate(_turretObj);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST 1: CanShootNow() - Logica di Sparo
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che CanShootNow() ritorna true quando il countdown è <= 0
    /// ✅ TESTABILE: Logica pura, no dipendenze
    /// </summary>
    [Test]
    public void CanShootNow_ReturnsTrue_WhenCountdownIsZero()
    {
        // ARRANGE
        var canShootMethod = typeof(TurretController).GetMethod("CanShootNow",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT: Passa countdown = 0
        bool result = (bool)canShootMethod.Invoke(_turretController, new object[] { 0f });

        // ASSERT
        Assert.IsTrue(result, "CanShootNow dovrebbe ritornare true quando countdown <= 0");
    }

    /// <summary>
    /// Verifica che CanShootNow() ritorna false quando il countdown è positivo
    /// </summary>
    [Test]
    public void CanShootNow_ReturnsFalse_WhenCountdownIsPositive()
    {
        // ARRANGE
        var canShootMethod = typeof(TurretController).GetMethod("CanShootNow",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT: Passa countdown = 0.5 (positivo)
        bool result = (bool)canShootMethod.Invoke(_turretController, new object[] { 0.5f });

        // ASSERT
        Assert.IsFalse(result, "CanShootNow dovrebbe ritornare false quando countdown > 0");
    }

    /// <summary>
    /// Verifica che CanShootNow() ritorna true quando il countdown è negativo
    /// (Questo potrebbe succedere se il giocatore accelera il tempo o simile)
    /// </summary>
    [Test]
    public void CanShootNow_ReturnsTrue_WhenCountdownIsNegative()
    {
        // ARRANGE
        var canShootMethod = typeof(TurretController).GetMethod("CanShootNow",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT: Passa countdown = -0.1 (negativo)
        bool result = (bool)canShootMethod.Invoke(_turretController, new object[] { -0.1f });

        // ASSERT
        Assert.IsTrue(result, "CanShootNow dovrebbe ritornare true quando countdown < 0");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST 2: ResetFireCountdown() - Reset del Timer
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che ResetFireCountdown() calcola correttamente il nuovo countdown
    /// Formula: 1 / fireRate
    /// Esempio: fireRate = 2 → countdown = 0.5 (un colpo ogni mezzo secondo)
    /// ✅ TESTABILE: Pura matematica
    /// </summary>
    [Test]
    public void ResetFireCountdown_CalculatesCorrectInterval()
    {
        // ARRANGE
        var resetMethod = typeof(TurretController).GetMethod("ResetFireCountdown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT: fireRate = 2 → countdown dovrebbe essere 0.5
        float result = (float)resetMethod.Invoke(_turretController, new object[] { 2f });

        // ASSERT
        Assert.AreEqual(0.5f, result, 0.0001f, "ResetFireCountdown(2) dovrebbe ritornare 0.5");
    }

    /// <summary>
    /// Verifica che ResetFireCountdown() funziona con fireRate = 1 (1 colpo al secondo)
    /// </summary>
    [Test]
    public void ResetFireCountdown_WithFireRateOne_ReturnsOne()
    {
        // ARRANGE
        var resetMethod = typeof(TurretController).GetMethod("ResetFireCountdown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // ACT: fireRate = 1 → countdown dovrebbe essere 1
        float result = (float)resetMethod.Invoke(_turretController, new object[] { 1f });

        // ASSERT
        Assert.AreEqual(1f, result, 0.0001f, "ResetFireCountdown(1) dovrebbe ritornare 1");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST 3: CalculateTargetRotation() - Logica di Rotazione
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che CalculateTargetRotation() crea una rotazione che punta verso il target
    /// Player a (5, 0, 0), Turret a (0, 0, 0) → Dovrebbe puntare a DESTRA (asse X positivo)
    /// ✅ TESTABILE: Logica pura, no dipendenze Transform
    /// </summary>
    [Test]
    public void CalculateTargetRotation_PointsTowardPlayer()
    {
        // ARRANGE
        var calcMethod = typeof(TurretController).GetMethod("CalculateTargetRotation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Vector3 playerPos = new Vector3(5f, 0f, 0f);      // Player a destra
        Vector3 turretPos = new Vector3(0f, 0f, 0f);      // Turret al centro
        float modelCorrection = 0f;                        // Nessuna correzione

        // ACT
        Quaternion result = (Quaternion)calcMethod.Invoke(_turretController, 
            new object[] { playerPos, turretPos, modelCorrection });

        // ASSERT: La rotazione dovrebbe puntare verso destra (asse X positivo)
        Vector3 lookDirection = result * Vector3.forward;
        // Normalizziamo per evitare problemi di floating point
        lookDirection.Normalize();
        
        // Dovrebbe puntare verso +X (a destra), non verso -X
        Assert.Greater(lookDirection.x, 0.9f, "La rotazione dovrebbe puntare a destra (X positivo)");
    }

    /// <summary>
    /// Verifica che CalculateTargetRotation() ignora la componente Y della direzione
    /// (La torretta non guarda verso l'alto/basso, solo orizzontalmente)
    /// </summary>
    [Test]
    public void CalculateTargetRotation_IgnoresYComponent()
    {
        // ARRANGE
        var calcMethod = typeof(TurretController).GetMethod("CalculateTargetRotation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Player ad ALTEZZA DIVERSA
        Vector3 playerPos = new Vector3(5f, 10f, 0f);     // 10 unità più in alto
        Vector3 turretPos = new Vector3(0f, 0f, 0f);
        float modelCorrection = 0f;

        // ACT
        Quaternion result = (Quaternion)calcMethod.Invoke(_turretController, 
            new object[] { playerPos, turretPos, modelCorrection });

        // ASSERT: La rotazione dovrebbe IGNORARE la Y e puntare comunque a destra
        Vector3 lookDirection = result * Vector3.forward;
        lookDirection.Normalize();
        
        // La Y della direzione di sguardo dovrebbe essere vicina a 0 (non guarda su/giù)
        Assert.Less(Mathf.Abs(lookDirection.y), 0.1f, "La rotazione non dovrebbe guardare verso l'alto/basso");
        
        // La X dovrebbe comunque puntare a destra
        Assert.Greater(lookDirection.x, 0.9f, "La rotazione dovrebbe puntare a destra nonostante la Y diversa");
    }

    /// <summary>
    /// Verifica che CalculateTargetRotation() applica la correzione del modello
    /// Se modelCorrection = 45 gradi, la rotazione finale dovrebbe essere ruotata di 45 gradi
    /// </summary>
    [Test]
    public void CalculateTargetRotation_AppliesToModelCorrection()
    {
        // ARRANGE
        var calcMethod = typeof(TurretController).GetMethod("CalculateTargetRotation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Vector3 playerPos = new Vector3(5f, 0f, 0f);
        Vector3 turretPos = new Vector3(0f, 0f, 0f);
        float modelCorrection = 45f;  // Correzione di 45 gradi

        // ACT: Calcola con correzione
        Quaternion resultWithCorrection = (Quaternion)calcMethod.Invoke(_turretController, 
            new object[] { playerPos, turretPos, modelCorrection });

        // Calcola SENZA correzione per confronto
        Quaternion resultWithoutCorrection = (Quaternion)calcMethod.Invoke(_turretController, 
            new object[] { playerPos, turretPos, 0f });

        // ASSERT: Le due rotazioni dovrebbero essere diverse
        Assert.AreNotEqual(resultWithCorrection, resultWithoutCorrection, 
            "La correzione del modello dovrebbe cambiare la rotazione finale");
    }

    /// <summary>
    /// Verifica che CalculateTargetRotation() ritorna identity quando player è nella stessa posizione
    /// </summary>
    [Test]
    public void CalculateTargetRotation_ReturnsIdentity_WhenDirectionIsZero()
    {
        // ARRANGE
        var calcMethod = typeof(TurretController).GetMethod("CalculateTargetRotation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Vector3 playerPos = new Vector3(0f, 0f, 0f);     // Stesso punto della torretta
        Vector3 turretPos = new Vector3(0f, 0f, 0f);
        float modelCorrection = 0f;

        // ACT
        Quaternion result = (Quaternion)calcMethod.Invoke(_turretController, 
            new object[] { playerPos, turretPos, modelCorrection });

        // ASSERT: Dovrebbe ritornare identity (nessuna rotazione)
        Assert.AreEqual(Quaternion.identity, result, 
            "Quando il player è nella stessa posizione, dovrebbe ritornare rotazione identity");
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// TURRET BULLET TESTS
// ═══════════════════════════════════════════════════════════════════════════════

public class TurretBulletTests
{
    private TurretBullet _bullet;
    private GameObject _bulletObj;

    [SetUp]
    public void Setup()
    {
        // CREA IL GAMEOBJECT DEL PROIETTILE
        _bulletObj = new GameObject("Bullet");
        _bullet = _bulletObj.AddComponent<TurretBullet>();
    }

    [TearDown]
    public void Teardown()
    {
        if (_bulletObj != null)
        {
            Object.DestroyImmediate(_bulletObj);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST 1: CalculateMovement() - Logica di Movimento
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che CalculateMovement() calcola correttamente il vettore di movimento
    /// Formula: Vector3.forward * speed * deltaTime
    /// ✅ TESTABILE: Pura matematica vettoriale
    /// </summary>
    [Test]
    public void CalculateMovement_CalculatesCorrectDistance()
    {
        // ARRANGE
        var calcMethod = typeof(TurretBullet).GetMethod("CalculateMovement",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float speed = 20f;
        float deltaTime = 0.1f;  // Un decimo di secondo

        // ACT: Dovrebbe muoversi 2 unità (20 * 0.1)
        Vector3 result = (Vector3)calcMethod.Invoke(_bullet, new object[] { speed, deltaTime });

        // ASSERT
        float expectedDistance = speed * deltaTime;  // 2.0f
        Assert.AreEqual(expectedDistance, result.z, 0.0001f, 
            "CalculateMovement dovrebbe ritornare forward * speed * deltaTime");
    }

    /// <summary>
    /// Verifica che CalculateMovement() ritorna movimento ZERO se deltaTime è zero
    /// (Nessun movimento se non passa tempo)
    /// </summary>
    [Test]
    public void CalculateMovement_ReturnsZero_WhenDeltaTimeIsZero()
    {
        // ARRANGE
        var calcMethod = typeof(TurretBullet).GetMethod("CalculateMovement",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float speed = 20f;
        float deltaTime = 0f;  // Nessun tempo passato

        // ACT
        Vector3 result = (Vector3)calcMethod.Invoke(_bullet, new object[] { speed, deltaTime });

        // ASSERT
        Assert.AreEqual(Vector3.zero, result, 
            "CalculateMovement dovrebbe ritornare zero se deltaTime è zero");
    }

    /// <summary>
    /// Verifica che CalculateMovement() è indipendente da X e Y (solo Z)
    /// Il proiettile si muove solo in "avanti" locale (Z), non lateralmente
    /// </summary>
    [Test]
    public void CalculateMovement_OnlyMovesForward()
    {
        // ARRANGE
        var calcMethod = typeof(TurretBullet).GetMethod("CalculateMovement",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float speed = 20f;
        float deltaTime = 0.1f;

        // ACT
        Vector3 result = (Vector3)calcMethod.Invoke(_bullet, new object[] { speed, deltaTime });

        // ASSERT: X e Y devono essere zero, solo Z ha movimento
        Assert.AreEqual(0f, result.x, "Il movimento non dovrebbe essere laterale (X)");
        Assert.AreEqual(0f, result.y, "Il movimento non dovrebbe essere verticale (Y)");
        Assert.Greater(result.z, 0f, "Il movimento dovrebbe essere in avanti (Z positivo)");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST 2: ShouldDestroyBullet() - Logica di Collisione
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che ShouldDestroyBullet() ritorna false per i trigger
    /// (I proiettili passano attraverso i trigger, come zone di attivazione)
    /// ✅ TESTABILE: Dipende da Collider.isTrigger (possiamo mockarlo)
    /// </summary>
    [Test]
    public void ShouldDestroyBullet_IgnoresTriggers()
    {
        // ARRANGE
        var shouldDestroyMethod = typeof(TurretBullet).GetMethod("ShouldDestroyBullet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Crea un GameObject con Collider trigger
        GameObject triggerObj = new GameObject("Trigger");
        Collider triggerCollider = triggerObj.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;

        bool playerHit = false;

        // ACT
        bool result = (bool)shouldDestroyMethod.Invoke(_bullet, 
            new object[] { triggerCollider, playerHit });

        // ASSERT: Non dovrebbe distruggere il proiettile su un trigger
        Assert.IsFalse(result, "ShouldDestroyBullet dovrebbe ignorare i trigger");

        // CLEANUP
        Object.DestroyImmediate(triggerObj);
    }

    /// <summary>
    /// Verifica che ShouldDestroyBullet() passa attraverso i Turret
    /// (Non distrugge il proiettile se colpisce la torretta che l'ha sparato)
    /// </summary>
    [Test]
    public void ShouldDestroyBullet_PassesThroughTurrets()
    {
        // ARRANGE
        var shouldDestroyMethod = typeof(TurretBullet).GetMethod("ShouldDestroyBullet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Crea un GameObject con tag "Turret"
        GameObject turretObj = new GameObject("Turret");
        turretObj.tag = "Turret";
        Collider turretCollider = turretObj.AddComponent<BoxCollider>();

        bool playerHit = false;

        // ACT
        bool result = (bool)shouldDestroyMethod.Invoke(_bullet, 
            new object[] { turretCollider, playerHit });

        // ASSERT: Non dovrebbe distruggere il proiettile se colpisce una torretta
        Assert.IsFalse(result, "ShouldDestroyBullet dovrebbe passare attraverso Turret");

        // CLEANUP
        Object.DestroyImmediate(turretObj);
    }

    /// <summary>
    /// Verifica che ShouldDestroyBullet() distrugge il proiettile su un muro
    /// (Collider non-trigger che non è Turret, Bullet, o Player)
    /// </summary>
    [Test]
    public void ShouldDestroyBullet_DestroyOnWalls()
    {
        // ARRANGE
        var shouldDestroyMethod = typeof(TurretBullet).GetMethod("ShouldDestroyBullet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Crea un GameObject generico (non ha tag speciale)
        GameObject wallObj = new GameObject("Wall");
        Collider wallCollider = wallObj.AddComponent<BoxCollider>();

        bool playerHit = false;

        // ACT
        bool result = (bool)shouldDestroyMethod.Invoke(_bullet, 
            new object[] { wallCollider, playerHit });

        // ASSERT: Dovrebbe distruggere il proiettile
        Assert.IsTrue(result, "ShouldDestroyBullet dovrebbe distruggere il proiettile su un muro");

        // CLEANUP
        Object.DestroyImmediate(wallObj);
    }
}