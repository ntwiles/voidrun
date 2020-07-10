
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ToastController : MonoBehaviour
{
    private Text textComponent;

    [SerializeField]
    private float toastDuration;
    private float toastTime;

    public delegate void ToastExpiredEventHandler(ToastController toast);
    public static event ToastExpiredEventHandler ToastExpiredEvent;

    public string Text
    {
        get
        {
            if (textComponent == null) getTextComponent();
            return textComponent.text;
        }
        set
        {
            if (textComponent == null) getTextComponent();
            textComponent.text = value;
        }
    }

    public static void ClearEventListeners()
    {
        // Wipe event listeners in case this isn't the first round of play.
        ToastExpiredEvent = null;
    }


    private void Update()
    {
        toastTime += Time.deltaTime;

        if (toastTime >= toastDuration)
        {
            ToastExpiredEvent(this);
        }
    }

    private void getTextComponent()
    {
        textComponent = GetComponent<Text>();
    }
}
