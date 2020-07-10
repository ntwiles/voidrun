

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class GameFinishedController : MonoBehaviour
{
    [SerializeField]
    private Text farthestShotText, bestStreakText, holesScoreText, timeBonusText, finalScoreText, highScoreText;

    [SerializeField]
    private Text newHighScoreAlert;

    [SerializeField]
    private GameObject trophyPrefab;

    [SerializeField]
    private int lineHeight;

    public void ShowData(float _farthestShot, int _bestStreak, int _holesScore, int _highScore, int _timeBonus, Dictionary<Trophy,int> _trophyData)
    {
        farthestShotText.text = _farthestShot.ToString();
        bestStreakText.text = _bestStreak.ToString();
        holesScoreText.text = _holesScore.ToString();
        timeBonusText.text = _timeBonus.ToString();

        int numTrophyTypes = Enum.GetNames(typeof(Trophy)).Length;

        var rootPosition = trophyPrefab.transform.localPosition;

        int numTrophies = 0;

        for (int i = 0; i < numTrophyTypes; i++)
        {
            Trophy trophy = (Trophy)i;

            if (_trophyData[trophy] > 0)
            {
                var trophyText = Instantiate(trophyPrefab,transform).GetComponent<Text>();

                trophyText.gameObject.SetActive(true);
                trophyText.text = trophy.ToString() + " x" + _trophyData[trophy];

                trophyText.transform.localPosition = rootPosition - new Vector3(0, numTrophies * lineHeight, 0);
                numTrophies++;
            }
        }

        int finalScore = _holesScore + _timeBonus;

        highScoreText.text = _highScore == -1 ? "0" : _highScore.ToString();
        finalScoreText.text = (_holesScore + _timeBonus).ToString();
        
        if (finalScore > _highScore)
        {
            newHighScoreAlert.gameObject.SetActive(true);
        }
    }
}
