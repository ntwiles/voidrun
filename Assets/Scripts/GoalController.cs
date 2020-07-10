using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField]
    Material materialActivated;

    private MeshRenderer renderer;

    private GameController gameController;

    public bool IsSunk;

    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    public void Trigger()
    {
        renderer.material = materialActivated;
        IsSunk = true;
    }
}
