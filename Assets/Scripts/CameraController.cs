using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    private CameraShaker shaker;

    private bool abandoningShip;
    private Vector3 abandonDirection;

    [SerializeField]
    private float abandonSpeed;

    void Start()
    {
        shaker = GetComponent<CameraShaker>();
    }

    void Update()
    {
        if (abandoningShip)
        {
            transform.position += abandonDirection * abandonSpeed * Time.deltaTime;
        }
    }

    public void AbandonShip(Vector3 dangerDirection)
    {
        transform.SetParent(null, true);
        shaker.enabled = false;
        abandoningShip = true;
        abandonDirection = dangerDirection * -1;
    }
}
