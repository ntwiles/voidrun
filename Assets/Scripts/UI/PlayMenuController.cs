
using UnityEngine;
using UnityEngine.UI;


public class PlayMenuController : MonoBehaviour
{
    [SerializeField]
    private Button accuracyButton, speedButton, backButton;

    public void EnableButtons()
    {
        accuracyButton.interactable = true;
        speedButton.interactable = true;
        backButton.interactable = true;
    }

    public void DisableButtons()
    {
        accuracyButton.interactable = false;
        speedButton.interactable = false;
        backButton.interactable = false;
    }
}

