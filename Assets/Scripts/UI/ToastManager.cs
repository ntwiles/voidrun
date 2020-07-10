
using System;
using System.Collections.Generic;

using UnityEngine;

public class ToastManager : MonoBehaviour
{
    private AudioSource bloop;

    private GameObject toastPrefab;
    private List<ToastController> toasts;

    [SerializeField]
    private float lineHeight;

    void Start()
    {
        bloop = GetComponent<AudioSource>();

        BallController.BallSunkEvent += onBallSunk;
        GameController.TrophyEarnedEvent += onTrophyEarned;

        toastPrefab = (GameObject)Resources.Load("Prefabs/UI/ToastPrefab");

        toasts = new List<ToastController>();

        ToastController.ToastExpiredEvent += onToastExpired;
    }

    private void onBallSunk(float distanceTravelled, bool thrown = true)
    {
        if (thrown) addToast("Hole sunk.");
        else addToast("Drive thru.");
    }

    private void onTrophyEarned(Trophy trophy)
    {
        string message;

        switch (trophy)
        {
            case Trophy.EpicShot: message = "Epic shot!"; break;
            case Trophy.LongShot: message = "Long shot!"; break;
            case Trophy.DoubleShot: message = "Double shot!"; break;
            case Trophy.TripleShot: message = "Triple shot!"; break;
            case Trophy.MultiShot: message = "Multi shot!"; break;
            case Trophy.QuadShot: message = "Quad shot!"; break;
            default: throw new NotImplementedException("We don't have a toast message for this trophy.");
        }

        addToast(message);
    }

    private void addToast(string message)
    {
        //Shift toasts to make room for this new one.
        moveToastsUp();

        //Create the new toast.
        ToastController toast = Instantiate(toastPrefab).GetComponent<ToastController>();
        toast.Text += message+"\n";
        toasts.Add(toast);

        //Position it.
        toast.gameObject.SetActive(true);
        toast.transform.SetParent(transform);
        toast.transform.localPosition = new Vector2(0, 0);
        toast.transform.localScale = Vector2.one;

        bloop.Play();
    }

    private void onToastExpired(ToastController toast)
    {
        toasts.Remove(toast);
        Destroy(toast.gameObject);
    }

    private void moveToastsUp()
    {
        foreach(var toast in toasts)
        {
            toast.transform.localPosition += new Vector3(0, lineHeight, 0);
        }
    }
}

public enum Trophy
{
    DoubleShot,
    TripleShot,
    QuadShot,
    MultiShot,
    LongShot,
    EpicShot
}
