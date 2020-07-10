using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class SpeedRadialController : MonoBehaviour
{
    [SerializeField]
    private Image radial;

    void Start()
    {
        radial = GetComponent<Image>();

        ShipController.SpeedChangedEvent += onPlayerSpeedChanged;
    }

    private void onPlayerSpeedChanged(float newSpeed, float maxSpeed)
    {
        radial.fillAmount = newSpeed / maxSpeed;
    }
}
