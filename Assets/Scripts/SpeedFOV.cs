
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SpeedFOV : MonoBehaviour
{
    private Camera camera;

    [SerializeField]
    private float fovMaxDisort;
    private float fovDistort;
    private float fovNormal;

    void Start()
    {
        camera = Camera.main;
        fovNormal = camera.fieldOfView;
    }

    public void UpdateProximityDistort(float percent)
    {
        fovDistort = fovMaxDisort * percent;
        camera.fieldOfView = fovNormal + fovDistort;
    }
}
