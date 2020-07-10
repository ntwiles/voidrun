
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private PlayMenuController playMenu;
    private Animator playMenuAnimator;

    [SerializeField]
    private Button playButton, settingsButton, quitButton;

    [SerializeField]
    private Button speedModeButton, accuracyModeButton, debugModeButton;

    private GameMode chosenGameMode;

    [SerializeField]
    private Text playModeDescriptionText, playModeTitleText;

    [SerializeField]
    private Text playMenuLoadingMessage;

    [SerializeField]
    private Button playMenuBackButton;

    private AudioSource menuClickSound;

    void Awake()
    {
        playMenuAnimator = playMenu.GetComponent<Animator>();
        menuClickSound = GetComponent<AudioSource>();
    }

    private void enableButtons()
    {
        playButton.interactable = true;
        settingsButton.interactable = false; // For now we want this disabled all the time.
        quitButton.interactable = true;
    }

    private void disableButtons()
    {
        playButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;
    }

    public void onClickPlayGame()
    {
        menuClickSound.Play();

        playMenu.EnableButtons();
        disableButtons();
        onClickPlayMenuSpeedMode();
        playMenuAnimator.SetBool("open", true);
    }

    public void onClickSettings()
    {

    }

    public void onClickQuitGame()
    {
        menuClickSound.Play();
        Application.Quit();
    }

    public void onClickPlayMenuDebugMode()
    {
        speedModeButton.interactable = true;
        accuracyModeButton.interactable = true;
        debugModeButton.interactable = false;

        chosenGameMode = GameMode.Debug;

        playModeDescriptionText.text = "What do you want me to say, Karen?\n\n";
        playModeTitleText.text = "DEBUG MODE";

        if (PlayerPrefs.HasKey(GameController.PREFS_KEY_HIGH_SCORE_DEBUG))
        {
            int highScore = PlayerPrefs.GetInt(GameController.PREFS_KEY_HIGH_SCORE_DEBUG);
            playModeDescriptionText.text += "High Score: " + highScore.ToString() + "\n";
        }

        if (PlayerPrefs.HasKey(GameController.PREFS_KEY_BEST_TIME_DEBUG))
        {
            int bestTime = PlayerPrefs.GetInt(GameController.PREFS_KEY_BEST_TIME_DEBUG);
            string bestTimeString = TimerMethods.FormatTimerString(bestTime);
            playModeDescriptionText.text += "Best Time: " + bestTimeString;
        }
    }

    public void onClickPlayMenuAccuracyMode()
    {
        speedModeButton.interactable = true;
        accuracyModeButton.interactable = false;
        debugModeButton.interactable = true;

        chosenGameMode = GameMode.Accuracy;

        playModeTitleText.text = "PRECISION MODE";
        playModeDescriptionText.text = "Take it easy and line up your shots. Score points by sinking holes at a longer distance.\n\nTip: Make sure to account for the ball's curve; it's affected by the gravity of the black hole.\n\n";

        if (PlayerPrefs.HasKey(GameController.PREFS_KEY_HIGH_SCORE_ACCURACY))
        {
            int highScore = PlayerPrefs.GetInt(GameController.PREFS_KEY_HIGH_SCORE_ACCURACY);
            playModeDescriptionText.text += "High Score: " + highScore.ToString() + "\n";
        }

        if (PlayerPrefs.HasKey(GameController.PREFS_KEY_BEST_TIME_ACCURACY))
        {
            int bestTime = PlayerPrefs.GetInt(GameController.PREFS_KEY_BEST_TIME_ACCURACY);
            string bestTimeString = TimerMethods.FormatTimerString(bestTime);
            playModeDescriptionText.text += "Best Time: " + bestTimeString;
        }

    }

    public void onClickPlayMenuSpeedMode()
    {
        speedModeButton.interactable = false;
        accuracyModeButton.interactable = true;
        debugModeButton.interactable = true;

        chosenGameMode = GameMode.Speed;

        playModeTitleText.text = "CHAOS MODE";
        playModeDescriptionText.text = "Fly at break-neck speeds, performing daring maneuvers in order to sink your holes as fast as humanly possible.\n\nTip: Stay near the black hole to maintain speed, but don't get sucked in!\n\n";

        if (PlayerPrefs.HasKey(GameController.PREFS_KEY_HIGH_SCORE_SPEED))
        {
            int highScore = PlayerPrefs.GetInt(GameController.PREFS_KEY_HIGH_SCORE_SPEED);
            playModeDescriptionText.text += "High Score: " + highScore.ToString() + "\n";
        }

        if (PlayerPrefs.HasKey(GameController.PREFS_KEY_BEST_TIME_SPEED))
        {
            int bestTime = PlayerPrefs.GetInt(GameController.PREFS_KEY_BEST_TIME_SPEED);
            string bestTimeString = TimerMethods.FormatTimerString(bestTime);
            playModeDescriptionText.text += "Best Time: " + bestTimeString;
        }
    }

    public void onClickPlayMenuBack()
    {
        playMenuAnimator.SetBool("open", false);
        playMenu.DisableButtons();
        enableButtons();
    }

    public void onClickPlayMenuPlayButton()
    {
        speedModeButton.interactable = false;
        accuracyModeButton.interactable = false;
        debugModeButton.interactable = false;
        playMenuBackButton.interactable = false;

        switch (chosenGameMode)
        {
            case GameMode.Accuracy: SceneManager.LoadScene("GameAccuracy"); break;
            case GameMode.Speed: SceneManager.LoadScene("GameSpeed"); break;
            case GameMode.Debug: SceneManager.LoadScene("GameDebug"); break;
        }

        playMenuLoadingMessage.gameObject.SetActive(true);
    }
}
