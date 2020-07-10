using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private int goalsLeft;

    // Score Metrics
    private int holesScore;
    private float farthestShot;
    private int currentStreak, longestStreak;
    private float timeSinceLastGoal;
    [SerializeField]
    private float streakDuration;
    private Dictionary<Trophy, int> trophiesEarned;

    public delegate void ScoreChangedEventHandler(float newScore, int goalsLeft);
    public static event ScoreChangedEventHandler ScoreChangedEvent;

    public delegate void TrophyEarnedEventHandler(Trophy trophy);
    public static event TrophyEarnedEventHandler TrophyEarnedEvent;

    public delegate void TimerUpdatedEventHandler(int timer);
    public static event TimerUpdatedEventHandler TimerUpdatedEvent;

    public delegate void GamePausedEventHandler();
    public static event GamePausedEventHandler GamePausedEvent;
    public static event GamePausedEventHandler GameResumedEvent;

    public delegate void GameEndedEventHandler();
    public static event GameEndedEventHandler GameEndedEvent;

    public delegate void GameLostEventHandler(GameOverReason reason);
    public static event GameLostEventHandler GameLostEvent;

    public delegate void GameWonEventHandler(float farthestShot, int longestStreak, int holesScore, int highScore, int timeBonus, Dictionary<Trophy,int> trophyData);
    public static event GameWonEventHandler GameWonEvent;

    public delegate void OfferWaypointsEventHandler(List<GoalController> targets);
    public static event OfferWaypointsEventHandler OfferWaypointsEvent;

    [SerializeField]
    public int LongShotDistance, EpicShotDistance;

    [SerializeField]
    private float longShotMultiplier, epicShotMultiplier, driveThruPoints;

    // Timer
    [SerializeField]
    private float longestTimeToScoreBonus, maximumTimeBonus;
    private float timerCurrent;

    private bool waitingToSpawnGoals;
    private float waitDuration, timeWaited;

    public static bool IsGamePaused, IsGameEnded;

    [SerializeField]
    private int waypointOfferThreshold;

    [SerializeField]
    EntitySpawner spawner;

    private List<GoalController> goals;
    private bool readyToOfferGoals;
    private bool offeredGoals;

    public const string PREFS_KEY_HIGH_SCORE_SPEED = "high_score_speed_mode";
    public const string PREFS_KEY_HIGH_SCORE_ACCURACY = "high_score_accuracy_mode";
    public const string PREFS_KEY_HIGH_SCORE_DEBUG = "high_score_debug_mode";
    public const string PREFS_KEY_BEST_TIME_SPEED = "best_time_speed_mode";
    public const string PREFS_KEY_BEST_TIME_ACCURACY = "best_time_accuracy_mode";
    public const string PREFS_KEY_BEST_TIME_DEBUG = "best_time_debug_mode";

    void Awake()
    {
        // Wipe event listeners in case this isn't the first round of play.
        clearEventListeners();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        BallController.BallSunkEvent += onBallSunk;
        EntitySpawner.GoalCreatedEvent += onGoalCreated;
        EntitySpawner.GoalsCreatedEvent += onGoalsCreated;

        waitingToSpawnGoals = true;
        waitDuration = 5;
        timeWaited = 0;

        initializeTrophyData();

        spawner = GameObject.FindGameObjectWithTag("EntitySpawner").GetComponent<EntitySpawner>();
    }

    void Start()
    {
        goals = new List<GoalController>();

        spawner.SpawnEntities();
    }

    // Update is called once per frame
    void Update()
    {
        timerCurrent += Time.deltaTime;
        TimerUpdatedEvent((int)(timerCurrent * 1000));

        if (waitingToSpawnGoals)
        {
            timeWaited += Time.deltaTime;

            if (timeWaited >= waitDuration)
            {
                goalsLeft = spawner.SpawnGoals();
                waitingToSpawnGoals = false;
            }
        }
        else
        {
            timeSinceLastGoal += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            if (!IsGameEnded)
            {
                if (IsGamePaused) triggerGameResumed();
                else triggerGamePaused();
            }
        }

        // Offer up targets to waypoint controller when there are few enough.
        if (goalsLeft <= waypointOfferThreshold && readyToOfferGoals)
        {
            OfferWaypointsEvent(goals);
            readyToOfferGoals = false;
            offeredGoals = true;
        }
    }

    private void onGoalCreated(GoalController goal)
    {
        goals.Add(goal);
    }

    private void onGoalsCreated()
    {
        readyToOfferGoals = true;
    }

    private void clearEventListeners()
    {
        // First wipe our own.
        TrophyEarnedEvent = null;
        ScoreChangedEvent = null;
        TimerUpdatedEvent = null;

        GameLostEvent = null;
        GameWonEvent = null;
        GameEndedEvent = null;
        GamePausedEvent = null;
        GameResumedEvent = null;

        OfferWaypointsEvent = null;

        // Then tell others to wipe theirs.
        BallController.ClearEventListeners();
        ToastController.ClearEventListeners();
        ShipController.ClearEventListeners();
    }

    private void initializeTrophyData()
    {
        trophiesEarned = new Dictionary<Trophy, int>();

        trophiesEarned.Add(Trophy.DoubleShot, 0);
        trophiesEarned.Add(Trophy.TripleShot, 0);
        trophiesEarned.Add(Trophy.QuadShot, 0);
        trophiesEarned.Add(Trophy.MultiShot, 0);
        trophiesEarned.Add(Trophy.LongShot, 0);
        trophiesEarned.Add(Trophy.EpicShot, 0);
    }

    public void onResumeButtonClicked()
    {
        triggerGameResumed();
    }

    public void onRestartButtonClicked()
    {
        // We od this because the spawner will persist into the next scene and could keep spawning goals otherwise.
        spawner.StopSpawning();

        triggerGameResumed();

        switch (spawner.CurrentGameMode)
        {
            case GameMode.Accuracy: SceneManager.LoadScene("GameAccuracy"); break;
            case GameMode.Speed: SceneManager.LoadScene("GameSpeed"); break;
            case GameMode.Debug: SceneManager.LoadScene("GameDebug"); break;
        }
    }

    public void onQuitButtonClicked()
    {
        triggerGameResumed();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // We do this because the spawner will persist into the next scene and could keep spawning goals otherwise.
        spawner.StopSpawning();
        SceneManager.LoadScene("MainMenu");
    }

    public void onBallSunk(float distanceTravelled, bool thrown = true)
    {
        // Mark one less goal.
        goalsLeft--;

        // Calculate points.
        float points = distanceTravelled;

        if (!thrown)
        {
            points += driveThruPoints;
        }
        else if (points >= EpicShotDistance)
        {
            points *= epicShotMultiplier;
            awardTrophy(Trophy.EpicShot);
        }
        else if (points >= LongShotDistance)
        {
            points *= longShotMultiplier;
            awardTrophy(Trophy.LongShot);
        }

        // Update metrics.
        if (distanceTravelled > farthestShot) farthestShot = distanceTravelled;

        if (timeSinceLastGoal <= streakDuration)
        {
            currentStreak++;
            awardStreakTrophy();
            if (currentStreak > longestStreak) longestStreak = currentStreak;
        }
        else currentStreak = 1;

        timeSinceLastGoal = 0;

        // Update the score and trigger the event to mark it as changed.
        holesScore += Mathf.RoundToInt(points);
        ScoreChangedEvent(holesScore, goalsLeft);

        // Check if game is over.
        if (goalsLeft < 1) triggerGameWon();

        if (offeredGoals) OfferWaypointsEvent(goals);
    }

    private void awardStreakTrophy()
    {
        Trophy trophy;

        switch (currentStreak)
        {
            case 2: trophy = Trophy.DoubleShot; break;
            case 3: trophy = Trophy.TripleShot; break;
            case 4: trophy = Trophy.QuadShot; break;
            default: trophy = Trophy.MultiShot; break;
        }

        awardTrophy(trophy);
    }

    private void awardTrophy(Trophy trophy)
    {
        trophiesEarned[trophy]++;
        TrophyEarnedEvent(trophy);
    }

    private void triggerGamePaused()
    {
        Time.timeScale = 0;
        IsGamePaused = true;
        GamePausedEvent();
    }

    private void triggerGameResumed()
    {
        Time.timeScale = 1;
        IsGamePaused = false;
        GameResumedEvent();
    }

    private void triggerGameWon()
    {
        string highScorePrefsKey = "";
        string bestTimePrefsKey = "";

        int standingHighScore = 0;
        int standingBestTime = 0;

        switch (spawner.CurrentGameMode)
        {
            case GameMode.Accuracy: highScorePrefsKey = PREFS_KEY_HIGH_SCORE_ACCURACY; bestTimePrefsKey = PREFS_KEY_BEST_TIME_ACCURACY; break;
            case GameMode.Speed: highScorePrefsKey = PREFS_KEY_HIGH_SCORE_SPEED; bestTimePrefsKey = PREFS_KEY_BEST_TIME_SPEED; break;
            case GameMode.Debug: highScorePrefsKey = PREFS_KEY_HIGH_SCORE_DEBUG; bestTimePrefsKey = PREFS_KEY_BEST_TIME_DEBUG; break;
        }

        //How close were we to maxxing out our allotted time?
        float ratioMaximumTime = timerCurrent / longestTimeToScoreBonus;

        //Clamp to a range of 0 to 1 and invert it.
        ratioMaximumTime = ratioMaximumTime > 1 ? 0 : 1 - ratioMaximumTime;

        int timeBonus = (int)Mathf.Round(maximumTimeBonus * ratioMaximumTime);

        standingBestTime = getStandingScore(bestTimePrefsKey);
        checkBestTime(bestTimePrefsKey, standingBestTime, (int)(timerCurrent * 1000));

        standingHighScore = getStandingScore(highScorePrefsKey);
        checkHighScore(highScorePrefsKey, standingHighScore, holesScore + timeBonus);

        IsGameEnded = true;
        GameEndedEvent();
        GameWonEvent(farthestShot, longestStreak, holesScore, standingHighScore, timeBonus, trophiesEarned);
    }

    public void TriggerGameLost(GameOverReason reason)
    {
        // We od this because the spawner will persist into the next scene and could keep spawning goals otherwise.
        spawner.StopSpawning();

        IsGameEnded = true;
        GameEndedEvent();
        GameLostEvent(reason);
    }

    private int getStandingScore(string key)
    {
        // Is there an existing high score?
        if (key != "" && PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetInt(key);
        }

        return -1;
    }

    private void checkBestTime(string key, int standingBestTime, int currentTime)
    {
        if (standingBestTime == -1 || currentTime < standingBestTime)
        {
            PlayerPrefs.SetInt(key, currentTime);
        }
    }

    private void checkHighScore(string key, int standingHighScore, int score)
    {
        if (score > standingHighScore)
        {
            PlayerPrefs.SetInt(key, score); 
        }
    }
}

public enum GameOverReason
{
    BlackHole,
    Asteroid
}
