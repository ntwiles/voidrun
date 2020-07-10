using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProximityAlertController : MonoBehaviour
{
    [SerializeField]
    private float flashOnDuration, flashOffDuration;

    private float flashTime;
    private bool isOn;

    private bool isFlashing;

    private Image icon;
    
    void Start()
    {
        ShipController.EnterBlackHoleProximityEvent += onEnterWarning;
        ShipController.ExitBlackHoleProximityEvent += onExitWarning;
        icon = GetComponent<Image>();
        flashTime = 0;

        isOn = false;
    }

    void Update()
    {
        if (isFlashing)
        {
            flashTime += Time.deltaTime;

            float duration = isOn ? flashOnDuration : flashOffDuration;

            if (flashTime >= duration)
            {
                toggleIcon();
                flashTime = 0;
            }
        }
    }

    private void onEnterWarning()
    {
        icon.enabled = true;
        isOn = true;
        isFlashing = true;
    }

    private void onExitWarning()
    {
        icon.enabled = false;
        isOn = false;
        isFlashing = false;
    }

    private void toggleIcon()
    {
        if (isOn)
        {
            Debug.Log("Turning flash off!");
            icon.enabled = false;
        }
        else
        {
            icon.enabled = true;
        }

        isOn = !isOn;
    }
}
