
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// This GameObject is the only object which is not destroyed between scenes.
public class EntitySpawner : MonoBehaviour
{
    private GameObject goalPrefab, blackHolePrefab, playerShipPrefab;
    private List<GameObject> asteroidPrefabs;

    // Black Holes
    public bool SpawnBlackHole;
    public float BlackHoleSize; // The size used to scale the black hole.
    private float blackHoleRadius; // The actual radius after it's been created. (Unity sphere scaling units do not equate to radius world units).
    private GameObject blackHole;

    // Asteroids
    public float MinAsteroidDistance, MaxAsteroidDistance;
    public bool SpawnAsteroids;
    public int MaxNumAsteroids;
    public float MinAsteroidRadius, MaxAsteroidRadius;

    // Goals
    public float MinGoalDistance { get; private set; }
    public float MaxGoalDistance { get; private set; }
    public int MaxNumGoals { get; private set; }
    private int numGoals;
    private bool isSpawningGoals;
    private float goalSpawnCounter;

    [SerializeField]
    private float goalSpawnDuration;
    private float goalSpawnRate;

    // Player
    public float PlayerSpawnDistance;

    [SerializeField]
    public GameMode CurrentGameMode;

    public delegate void GoalCreatedEventHandler(GoalController goal);
    public static event GoalCreatedEventHandler GoalCreatedEvent;

    public delegate void GoalsCreatedEventHandler();
    public static event GoalsCreatedEventHandler GoalsCreatedEvent;

    void Awake()
    {
        blackHolePrefab = (GameObject)Resources.Load("Prefabs/BlackHolePrefab");
        playerShipPrefab = (GameObject)Resources.Load("Prefabs/PlayerShipPrefab");
        goalPrefab = (GameObject)Resources.Load("Prefabs/GoalPrefab");
        asteroidPrefabs = loadAsteroidPrefabs();

        initializePresets();
    }

    void Update()
    {
        if (isSpawningGoals)
        {
            goalSpawnCounter += Time.deltaTime;

            if (goalSpawnCounter >= goalSpawnRate)
            {
                spawnGoal();

                if (numGoals >= MaxNumGoals) isSpawningGoals = false;

                goalSpawnCounter = 0;
            }
        }
    }

    public void StopSpawning()
    {
        isSpawningGoals = false;
        numGoals = 0;
    }

    private void initializePresets()
    {
        switch (CurrentGameMode)
        {
            case GameMode.Accuracy:
                {
                    // Black Hole
                    SpawnBlackHole = true;
                    BlackHoleSize = 200;

                    // Asteroids
                    SpawnAsteroids = true;
                    MinAsteroidDistance = 150;
                    MaxAsteroidDistance = 450;
                    MinAsteroidRadius = 5;
                    MaxAsteroidRadius = 15;
                    MaxNumAsteroids = 750;

                    // Goals
                    MinGoalDistance = 200;
                    MaxGoalDistance = 300;
                    MaxNumGoals = 30;

                    // Player
                    PlayerSpawnDistance = 500;

                    break;
                }
            case GameMode.Speed:
                {
                    // Black Hole
                    SpawnBlackHole = true;
                    BlackHoleSize = 200;

                    // Asteroids
                    SpawnAsteroids = true;
                    MinAsteroidDistance = 30;
                    MaxAsteroidDistance = 250;
                    MinAsteroidRadius = 5;
                    MaxAsteroidRadius = 10;
                    MaxNumAsteroids = 750;

                    // Goals
                    MinGoalDistance = 20;
                    MaxGoalDistance = 40;
                    MaxNumGoals = 30;

                    // Player
                    PlayerSpawnDistance = 400;

                    break;
                }
            case GameMode.Debug:
                {
                    // Black Hole
                    SpawnBlackHole = false;

                    // Asteroids
                    SpawnAsteroids = false;

                    // Goals
                    MinGoalDistance = 10;
                    MaxGoalDistance = 10;
                    MaxNumGoals = 2;

                    // Player
                    PlayerSpawnDistance = 300;

                    break;
                }
        }
    }

    public void SpawnEntities()
    {
        Debug.Log("Spawning Game: " + CurrentGameMode);

        numGoals = 0;
        blackHole = null;

        if (SpawnBlackHole) spawnBlackHole();
        if (SpawnAsteroids) spawnAsteroids();
        spawnPlayerShip();
    }

    private List<GameObject> loadAsteroidPrefabs()
    {
        List<GameObject> asteroids = new List<GameObject>();

        for (int i = 0; i < 21; i++)
        {
            asteroids.Add((GameObject)Resources.Load("Models/Asteroids/Prefabs/astre_" + i));
        }

        return asteroids;
    }

    private void spawnBlackHole()
    {
        blackHole = GameObject.Instantiate(blackHolePrefab);
        blackHole.transform.localScale = new Vector3(BlackHoleSize, BlackHoleSize, BlackHoleSize);

        blackHoleRadius = blackHole.GetComponent<SphereCollider>().bounds.extents.x;
    }

    private void spawnAsteroids()
    {
        for (int i = 0; i < MaxNumAsteroids; i++)
        {
            // Select a position.
            float distance = UnityEngine.Random.Range(blackHoleRadius + MinAsteroidDistance, blackHoleRadius + MaxAsteroidDistance);
            Vector3 asteroidPosition = UnityEngine.Random.onUnitSphere * distance;

            // Select a prefab.
            int prefabIndex = UnityEngine.Random.Range(0, 20);

            // Instantiate and position the asteroid.
            GameObject asteroid = (GameObject)GameObject.Instantiate(asteroidPrefabs[prefabIndex]);
            asteroid.transform.position = asteroidPosition;
            asteroid.SetActive(true);

            // Size the asteroid.
            float asteroidRadius = UnityEngine.Random.Range(MinAsteroidRadius, MaxAsteroidRadius);
            asteroid.transform.localScale = new Vector3(asteroidRadius, asteroidRadius, asteroidRadius);
        }
    }

    public int SpawnGoals()
    {
        isSpawningGoals = true;

        goalSpawnRate = goalSpawnDuration / MaxNumGoals;
        goalSpawnCounter = 0;

        GoalsCreatedEvent();

        return MaxNumGoals;
    }

    private void spawnGoal()
    {
        // TODO: We want to grab the component every time? Is that slow?
        var sphereCollider = goalPrefab.GetComponent<SphereCollider>();
        float radius = sphereCollider.radius * sphereCollider.transform.lossyScale.x;
        bool goalCreated = false;
        int maxAttempts = 50;
        int attemptsMade = 0;

        while (!goalCreated && attemptsMade < maxAttempts)
        {
            float distance = UnityEngine.Random.Range(blackHoleRadius + MinGoalDistance, blackHoleRadius + MaxGoalDistance);

            Vector3 goalPosition = UnityEngine.Random.onUnitSphere * distance;

            if (!Physics.CheckSphere(goalPosition, radius))
            {
                GoalController goal = GameObject.Instantiate(goalPrefab).GetComponent<GoalController>();
                goal.transform.position = goalPosition;
                goal.gameObject.SetActive(true);
                goalCreated = true;
                numGoals++;

                GoalCreatedEvent(goal);
                return;
            }

            attemptsMade++;
        }

        throw new Exception("Failed all attempts to spawn a goal.");
    }

    private void spawnPlayerShip()
    {
        var ship = GameObject.Instantiate(playerShipPrefab);
        ship.transform.position += new Vector3(PlayerSpawnDistance, 0, 0);
        ship.SetActive(true);
    }
}

public enum GameMode
{
    Speed,
    Accuracy,
    Debug
}