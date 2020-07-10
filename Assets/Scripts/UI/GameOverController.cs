

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [SerializeField]
    private Text reasonText, tipText;

    public void ShowData(GameOverReason reason)
    {
        switch (reason)
        {
            case GameOverReason.BlackHole:
                {
                    reasonText.text = "THE BLACK HOLE TOOK YOU.";
                    tipText.text = "Tip: Flying near the black hole is a great way to increase your speed, just make sure not to get too close.";
                    break;
                }
            case GameOverReason.Asteroid:
                {
                    reasonText.text = "YOU HIT AN ASTEROID.";
                    tipText.text = "Tip: You can reduce your speed by avoiding the black hole.";
                    break;
                }
        }
    }
}