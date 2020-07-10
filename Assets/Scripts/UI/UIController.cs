
using System;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject gameFinishedPanel, gameOverPanel, gamePausedPanel;

    [SerializeField]
    GameObject toastManager, speedRadial, healthRadial, scoreElement, goalsLeftText, timerElement;

    [SerializeField]
    Text timerText;
    private bool shouldUpdateTime;

    void Start()
    {
        GameController.GameEndedEvent += onGameEnded;
        GameController.GameWonEvent += onGameWon;
        GameController.GameLostEvent += onGameLost;
        GameController.TimerUpdatedEvent += onTimerUpdated;
        GameController.GamePausedEvent += onGamePaused;
        GameController.GameResumedEvent += onGameResumed;

        shouldUpdateTime = true;
    }

    private void hideGameplayUI()
    {
        toastManager.SetActive(false);
        speedRadial.SetActive(false);
        healthRadial.SetActive(false);
        scoreElement.SetActive(false);
        goalsLeftText.SetActive(false);
        timerElement.SetActive(false);
    }

    private void showGameplayUI()
    {
        toastManager.SetActive(true);
        speedRadial.SetActive(true);
        healthRadial.SetActive(true);
        scoreElement.SetActive(true);
        goalsLeftText.SetActive(true);
        timerElement.SetActive(true);
    }

    private void onGamePaused()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        hideGameplayUI();
        gamePausedPanel.SetActive(true);
    }

    private void onGameResumed()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        showGameplayUI();
        gamePausedPanel.SetActive(false);
    }

    private void onGameEnded()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        hideGameplayUI();

        shouldUpdateTime = false;
    }

    private void onGameWon(float farthestShot, int longestStreak, int holesScore, int highScore, int timeBonus, Dictionary<Trophy,int> trophyData)
    {
        gameFinishedPanel.SetActive(true);
        gameFinishedPanel.GetComponent<GameFinishedController>().ShowData(farthestShot, longestStreak, holesScore, highScore, timeBonus, trophyData);
    }

    private void onGameLost(GameOverReason reason)
    {
        gameOverPanel.SetActive(true);
        gameOverPanel.GetComponent<GameOverController>().ShowData(reason);
    }

    private void onTimerUpdated(int milliseconds)
    {
        if (shouldUpdateTime)
        {
            timerText.text = TimerMethods.FormatTimerString(milliseconds);
        }
    }
}
