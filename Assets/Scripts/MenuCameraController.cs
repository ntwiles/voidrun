using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [SerializeField]
    [Range(1, 5)]
    float bounceScale;

    [SerializeField]
    [Range(1,3)]
    float bounceFrequency;

    float bounceTime;
    Vector3 startPosition;

    void Start()
    {
        bounceTime = 0;
        startPosition = transform.position;
    }


    void Update()
    {
        bounceTime += Time.deltaTime;

        float yPos = Mathf.Sin(bounceTime * bounceFrequency) * bounceScale;

        transform.position = new Vector3(startPosition.x, yPos, startPosition.z);
    }
}
