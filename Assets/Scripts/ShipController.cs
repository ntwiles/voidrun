
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    // Speed
    [Header("Speed Settings")]

    [SerializeField]
    private float baseSpeed;
    [SerializeField]
    private float maxSpeed, turnSpeed, strafeXSpeed, strafeYSpeed, rollSpeed;

    [SerializeField]
    private float resetPositionSpeed, resetRotationSpeed;

    public float CurrentSpeed { get; private set; }

    // Other Settings
    [Header("Other Settings")]
    [SerializeField]
    private float minSafeDistance;
    [SerializeField]
    private float minWarningDistance;

    [SerializeField]
    public float DistanceToBlackHole, MinSpeedBoostDistance, MaxSpeedBoostDistance;

    [SerializeField]
    private float blindspotAngle;

    // Ship
    private Quaternion shipStartRotation;

    // Ball
    private GameObject ballPrefab;
    private BallController currentBall;

    [SerializeField]
    private float ballCooldownDuration;

    private float ballCooldownCurrent;

    [SerializeField]
    public Vector3 RelativeBallPosition;

    // State
    [Header("State Settings")]
    [SerializeField]
    private float engineDeathDuration;
    private bool isEngineKilled;
    private float engineDeathTime;
    private float engineSoundStartVolume;
    private float currentDamage;
    private bool isNearBlackHole;

    // Collisions
    [Header("Collision Settings")]
    [SerializeField]
    private float asteroidBounceMultiplier;

    [SerializeField]
    private float collisionMaxForce;
    [SerializeField]
    [Range(-1, 1)]
    private float collisionMaxAngle;

    // Audio
    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip engineHumClip;
    [SerializeField]
    private AudioClip collisionClip, proximityAlertClip, ballSinkClip, musicClipSpeed, musicClipAccuracy;
    private AudioSource engineHumSound, collisionSound, proximityAlertSound, ballSinkSound, musicSound;

    // Dependencies
    [Header("Dependencies")]

    [SerializeField]
    private GameObject ship;

    private EntitySpawner spawner;

    [SerializeField]
    private CameraController camera;

    [SerializeField]
    private CameraShaker shaker;

    [SerializeField]
    private SpeedFOV speedFOV;

    // Foreign components
    private GameObject blackHole;
    private GameController gameController;

    // Local Components
    private Rigidbody rigidBody;

    // Child Components
    [SerializeField]
    private ParticleSystem speedParticles;

    // Events
    public delegate void SpeedChangedEventHandler(float newSpeed, float maxSpeed);
    public static event SpeedChangedEventHandler SpeedChangedEvent;

    public delegate void PositionChangedEventHandler(Vector3 newPosition, Quaternion newRotation);
    public static event PositionChangedEventHandler PositionChangedEvent;

    public delegate void ShipDamagedEventHandler(float health, float maxHealth);
    public static event ShipDamagedEventHandler ShipDamagedEvent;

    public delegate void BlackHoleProximityEventHandler();
    public static event BlackHoleProximityEventHandler EnterBlackHoleProximityEvent;
    public static event BlackHoleProximityEventHandler ExitBlackHoleProximityEvent;

    public static void ClearEventListeners()
    {
        // Wipe event listeners in case this isn't the first round of play.
        SpeedChangedEvent = null;
        ShipDamagedEvent = null;
        PositionChangedEvent = null;
        EnterBlackHoleProximityEvent = null;
        ExitBlackHoleProximityEvent = null;
    }

    void Awake()
    {
        // Get local components.
        rigidBody = GetComponent<Rigidbody>();

        ballPrefab = (GameObject)Resources.Load("Prefabs/BallPrefab");

        spawner = GameObject.FindGameObjectWithTag("EntitySpawner").GetComponent<EntitySpawner>();

        engineHumSound = AddAudio(engineHumClip, true, true, .2f);
        collisionSound = AddAudio(collisionClip, false, false, 1f);
        proximityAlertSound = AddAudio(proximityAlertClip, true, true, .5f);
        ballSinkSound = AddAudio(ballSinkClip, false, false, 1);

        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        GameController.GameEndedEvent += onGameEnded;
        GameController.GamePausedEvent += onGamePaused;
        GameController.GameResumedEvent += onGameResumed;
        BallController.BallSunkEvent += onBallSunk;

        engineSoundStartVolume = engineHumSound.volume;

        CurrentSpeed = baseSpeed;

        shipStartRotation = ship.transform.localRotation;

        currentDamage = 0;
    }

    void Start()
    {
        blackHole = GameObject.FindGameObjectWithTag("BlackHole");

        AudioClip musicClip = null;

        switch (spawner.CurrentGameMode)
        {
            case GameMode.Accuracy: musicClip = musicClipAccuracy; break;
            case GameMode.Debug: musicClip = musicClipAccuracy; break;
            case GameMode.Speed: musicClip = musicClipSpeed; break;
        }

        musicSound = AddAudio(musicClip, true, true, .5f);

        engineHumSound.Play();
        musicSound.Play();

        generateBall();
    }

    void Update()
    {
        if (GameController.IsGamePaused) return;

        if (!isEngineKilled)
        {
            handleSpeed();
            handleMovementControls();
            handleAbilityControls();

            if (DistanceToBlackHole <= minWarningDistance && !isNearBlackHole)
            {
                isNearBlackHole = true;
                if (!proximityAlertSound.isPlaying) proximityAlertSound.Play();
                EnterBlackHoleProximityEvent();
            }

            if (DistanceToBlackHole > minWarningDistance && isNearBlackHole)
            {
                isNearBlackHole = false;
                if (proximityAlertSound.isPlaying) proximityAlertSound.Stop();
                ExitBlackHoleProximityEvent();
            }
        }
        else
        {
            engineDeathTime += Time.deltaTime;
            float engineDeathPercent = engineDeathTime / engineDeathDuration;
            engineHumSound.volume = Mathf.Lerp(engineSoundStartVolume, 0, engineDeathPercent);
        }
    }

    private AudioSource AddAudio(AudioClip clip, bool loop, bool playAwake, float vol)
    {
        var newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.clip = clip;
        newAudio.loop = loop;
        newAudio.playOnAwake = playAwake;
        newAudio.volume = vol;
        return newAudio;
    }

    private void handleMovementControls()
    {
        handleRotation();
        handleTranslation();
    }

    private void handleRotation()
    {
        float yaw = Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime;
        float pitch = Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime;
        float roll = Input.GetAxis("Roll") * rollSpeed * Time.deltaTime;

        Vector3 rotation = new Vector3(pitch, yaw, roll);

        transform.Rotate(rotation);
    }

    private void handleTranslation()
    {
        // Strafing
        var strafeX = Input.GetAxis("Horizontal") * transform.right * strafeXSpeed * Time.deltaTime * -1;
        var strafeY = Input.GetAxis("Vertical") * transform.up * strafeYSpeed * Time.deltaTime;
        var propulsion = transform.forward * CurrentSpeed * Time.deltaTime * -1;

        var motion = strafeX + strafeY + propulsion;

        rigidBody.AddForce(motion,ForceMode.VelocityChange);
    }

    private void handleAbilityControls()
    {
        // Throw ball.
        if (Input.GetButtonDown("Fire1") && currentBall && !isEngineKilled)
        //if (Input.GetMouseButtonDown(0) && currentBall && !isEngineKilled)
        {
            if (currentBall.Throw(transform.forward * -1))
            {
                currentBall = null;
                ballCooldownCurrent = 0;
            }
        }

        if (currentBall == null && !isEngineKilled)
        {
            ballCooldownCurrent += Time.deltaTime;

            if (ballCooldownCurrent >= ballCooldownDuration)
            {
                generateBall();
            }
        }
        else
        {
            currentBall.transform.localPosition = RelativeBallPosition;
        }
    }

    private void handleSpeed()
    {
        if (blackHole == null)
        {
            CurrentSpeed = baseSpeed;
        }
        else
        {
            // Distance to black hole.
            DistanceToBlackHole = Vector3.Distance(blackHole.transform.position, transform.position);

            float speedBoostDistance = DistanceToBlackHole - MinSpeedBoostDistance;

            float speedBonus = 0;
            float boostRatio = 0;

            if (speedBoostDistance <= MaxSpeedBoostDistance)
            {
                float distanceRatio = speedBoostDistance / MaxSpeedBoostDistance;

                boostRatio = 1 - distanceRatio;
                speedBonus = maxSpeed * boostRatio;
            }

            CurrentSpeed = baseSpeed + speedBonus;

            handleSpeedFOV(boostRatio);
            handleSpeedParticles(boostRatio);
        }

        SpeedChangedEvent(CurrentSpeed, maxSpeed);
    }

    private void handleSpeedFOV(float percent)
    {
        speedFOV.UpdateProximityDistort(percent);
    }

    private void handleSpeedParticles(float percent)
    {
        float maxEmission = 30;

        var emission = speedParticles.emission;
        emission.rateOverTime = percent * maxEmission;
    }

    private void generateBall()
    {
        currentBall = GameObject.Instantiate(ballPrefab).GetComponent<BallController>();

        if (blackHole != null)
        {
            currentBall.SetBlackHolePosition(blackHole.transform.position);
        }

        currentBall.gameObject.SetActive(true);

        currentBall.transform.SetParent(transform);
        currentBall.Spawn();
    }

    private void triggerGameLost(GameOverReason reason)
    {
        camera.AbandonShip(rigidBody.velocity.normalized);
        gameController.TriggerGameLost(reason);
    }

    private void onGameEnded() 
    {
        pauseSounds();
        killEngine(); 
    }

    private void onBallSunk(float a, bool b)
    {
        ballSinkSound.Play();
    }

    private void onGamePaused()
    {
        pauseSounds();
    }

    private void onGameResumed()
    {
        resumeSounds();
    }

    private void pauseSounds()
    {
        musicSound.Pause();
        engineHumSound.Pause();
        proximityAlertSound.Pause();
        collisionSound.Pause();
        ballSinkSound.Pause();
    }

    private void resumeSounds()
    {
        musicSound.UnPause();
        engineHumSound.UnPause();
        proximityAlertSound.UnPause();
        collisionSound.UnPause();
        ballSinkSound.UnPause();
    }

    private void damageShip(float damage)
    {
        currentDamage += damage;

        ShipDamagedEvent(1 - currentDamage, 1);

        if (currentDamage >= 1)
        {
            triggerGameLost(GameOverReason.Asteroid);
        }
    }

    private void killEngine()
    {
        isEngineKilled = true;
        var emission = speedParticles.emission;
        emission.enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Collision with black hole!");

        if (collider.tag == "BlackHole")
        {
            triggerGameLost(GameOverReason.BlackHole);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Asteroid")
        {
            Vector3 asteroidNormal = getOtherColliderNormal(collision);
            Vector3 direction = transform.forward.normalized * -1;

            // Dot returns 1 if they point in exactly the same direction,
            // -1 if they point in completely opposite directions,
            // and zero if the vectors are perpendicular.
            float dotProduct = Vector3.Dot(asteroidNormal, direction);

            float angleFactor = -dotProduct;
            float velocityFactor = collision.relativeVelocity.magnitude / collisionMaxForce;

            float damage = angleFactor * velocityFactor;

            damageShip(damage);

            Vector3 moveAwayDir = Vector3.Reflect(transform.forward.normalized * -1, asteroidNormal);
            rigidBody.AddForce(moveAwayDir * asteroidBounceMultiplier, ForceMode.VelocityChange);

            collisionSound.Play();
        }
    }

    private Vector3 getOtherColliderNormal(Collision collision)
    {
        // find collision point and normal. You may want to average over all contacts
        Vector3 point = collision.contacts[0].point;

        // you need vector pointing TOWARDS the collision, not away from it
        Vector3 dir = -collision.contacts[0].normal; 
                                            
        // step back a bit
        point -= dir;
        RaycastHit hitInfo;

        // cast a ray twice as far as your step back. This seems to work in all
        // situations, at least when speeds are not ridiculously big
        if (collision.collider.Raycast(new Ray(point, dir), out hitInfo, 2))
        {
            return hitInfo.normal;
        }

        return Vector3.zero;
    }
}
