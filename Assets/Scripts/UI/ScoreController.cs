using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField]
    private Text Text;

    // Start is called before the first frame update
    void Start()
    {
        Text = GetComponent<Text>();
        GameController.ScoreChangedEvent += onScoreChanged;
    }

    // Update is called once per frame
    void onScoreChanged(float newScore, int goalsLeft)
    {
        Text.text = newScore.ToString();
    }
}
