
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class WaypointMarker : MonoBehaviour
{
    Image waypoint;
    List<Waypoint> waypoints;

    GameObject canvas;

    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        waypoints = new List<Waypoint>();

        GameController.OfferWaypointsEvent += onOfferWaypoints;
    }

    private void onOfferWaypoints(List<GoalController> goals)
    {
        Debug.Log("Waypoints offered!");

        foreach(var waypoint in waypoints)
        {
            Destroy(waypoint.Marker.gameObject);
        }

        waypoints = new List<Waypoint>();

        foreach(GoalController goal in goals)
        {
            if (!goal.IsSunk) addWaypoint(goal.transform);
        }
    }

    private void addWaypoint(Transform target)
    {
        Debug.Log("Creating waypoint!");

        var waypointGO = (GameObject)Instantiate(Resources.Load("Prefabs/UI/WaypointPrefab"));

        waypoint = waypointGO.GetComponent<Image>();
        waypoint.transform.SetParent(canvas.transform);

        waypoints.Add(new Waypoint(waypoint, target));
    }

    void Update()
    {
        foreach(var waypoint in waypoints)
        {
            updateWaypoint(waypoint);
        }
    }

    private void updateWaypoint(Waypoint waypoint)
    {
        float minX = waypoint.Marker.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = waypoint.Marker.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector3 offset = new Vector3(0, 10, 0);

        Vector2 targetPoint = Camera.main.WorldToScreenPoint(waypoint.Target.position);
        Vector2 position = Camera.main.WorldToScreenPoint(waypoint.Target.position + offset);

        if (Vector3.Dot((waypoint.Target.position - transform.position), transform.forward * -1) < 0)
        {
            // Target is behind the player.
            if (position.x < Screen.width / 2)
            {
                position.x = maxX;
            }
            else
            {
                position.x = minX;
            }
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        waypoint.Marker.transform.position = position;
    }
}
