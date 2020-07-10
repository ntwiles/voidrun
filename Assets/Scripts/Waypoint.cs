
using UnityEngine;
using UnityEngine.UI;


struct Waypoint
{
    public Image Marker;
    public Transform Target;

    public Waypoint(Image marker, Transform target)
    {
        Marker = marker;
        Target = target;
    }
}

