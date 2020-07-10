using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    // Settings 
    [SerializeField]
    private float fallSpeed, throwForce;
    [SerializeField]
    private float maxTravelDistance;

    // Effects
    [SerializeField]
    private ParticleSystem destructionParticles;

    // Components 
    private AudioSource fireSound;
    private Rigidbody rigidBody;

    // State
    private bool isThrown;
    private bool isSunk;
    private Vector3 lastPosition;
    private float distanceTravelled;
    private bool hasBlackHole;
    private Vector3 blackHolePosition;

    // Events
    public delegate void BallSunkEventHandler(float distanceTravelled, bool thrown = true);
    public static event BallSunkEventHandler BallSunkEvent;

    // Spawning
    [SerializeField]
    private float maxRadius, spawnDuration;
    private float spawnTime;
    private bool isSpawning;

    public static void ClearEventListeners()
    {
        // Wipe event listeners in case this isn't the first round of play.
        BallSunkEvent = null;
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        fireSound = GetComponent<AudioSource>();

        spawnTime = 0;
    }

    public void SetBlackHolePosition(Vector3 position)
    {
        blackHolePosition = position;
        hasBlackHole = true;
    }

    void Update()
    {
        if (isSpawning)
        {
            spawnTime += Time.deltaTime;

            float spawnRatio = spawnTime / spawnDuration;

            float currentRadius = spawnRatio * maxRadius;

            transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);

            if (spawnTime >= spawnDuration) isSpawning = false;
        }
        else if (isThrown && !isSunk)
        {
            if (hasBlackHole)
            {
                Vector3 direction = blackHolePosition - transform.position;

                // Gravity force
                rigidBody.AddForce(direction * fallSpeed * Time.deltaTime);
            }

            distanceTravelled += Vector3.Distance(transform.position, lastPosition);

            if (distanceTravelled > maxTravelDistance)
            {
                destroyBall(rigidBody.velocity);
            }

            lastPosition = transform.position;
        }
    }

    public void Spawn()
    {
        isSpawning = true;
    }

    public bool Throw(Vector3 direction)
    {
        if (!isThrown && !isSpawning)
        {
            isThrown = true;
            rigidBody.AddForce(direction * throwForce, ForceMode.Impulse);

            // We hold the world scale in a buffer in case its local scale has been manipulated by its parent.
            var scale = transform.lossyScale;
            transform.parent = null;
            transform.localScale = scale;

            fireSound.Play();

            lastPosition = transform.position;
            distanceTravelled = 0;

            return true;
        }

        return false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            GoalController goal = other.GetComponent<GoalController>();

            if (!goal.IsSunk)
            {
                goal.Trigger();

                // If we're still holding onto the ball, we get the sink, but we don't want to kill it.
                if (isThrown)
                {
                    Destroy(rigidBody);
                    Destroy(GetComponent<Collider>());
                    transform.position = goal.transform.position;
                    fallSpeed = 0;
                    isSunk = true;
                    BallSunkEvent(distanceTravelled);
                }
                else
                {
                    BallSunkEvent(distanceTravelled, false);
                }

            }
        }

        if (other.tag == "BlackHole")
        {
            GameObject.Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;

        if (other.tag == "Asteroid")
        {
            destroyBall(collision.contacts[0].normal);
        }
    }

    private void destroyBall(Vector3 direction)
    {
        var particles = Instantiate(destructionParticles,transform.position,Quaternion.FromToRotation(Vector3.forward,direction));
        GameObject.Destroy(gameObject);
    }
}
