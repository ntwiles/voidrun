
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

public class GoalsLeftController : MonoBehaviour
{
    private Text text;

    private EntitySpawner entitySpawner;

    void Start()
    {
        text = GetComponent<Text>();
        GameController.ScoreChangedEvent += onScoreChanged;

        entitySpawner = GameObject.FindGameObjectWithTag("EntitySpawner").GetComponent<EntitySpawner>();

        text.text = entitySpawner.MaxNumGoals.ToString();
    }

    private void onScoreChanged(float newScore, int goalsLeft)
    {
        text.text = goalsLeft.ToString();
    }
}
