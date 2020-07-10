
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private Transform camera;

    [SerializeField]
    private float proximityShakeMaxPower;
    private float proximityShakePower;

    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.transform;
        startPosition = camera.localPosition;
    }

    void Update()
    {
        camera.localPosition = startPosition + UnityEngine.Random.insideUnitSphere * proximityShakePower * Time.deltaTime;
    }

    public void BurstShake()
    {
        throw new NotImplementedException();
    }

    public void UpdateProximityShake(float percent)
    {
        proximityShakePower = proximityShakeMaxPower * percent;

    }
}
