using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OffRoadChecker : MonoBehaviour
{
    public CarController player;

    private List<GameObject> currentCollisions = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<CarController>();
    }

    bool isOnTrack()
    {
        foreach (GameObject collidedObject in currentCollisions)
        {
            if (collidedObject.tag == "Track")
            {
                return true;
            }
        }
        return false;
    }

    // Detects if the car has left the track
    void OnTriggerExit(Collider collider)
    {
        print("Checker: " + collider.gameObject.name + ", " + collider.gameObject.tag);
        currentCollisions.Remove(collider.gameObject);
        if (!isOnTrack())
        {
            // player.resetPosition();
            print("crashed");
            bool crashed = true;
            player.SendValue();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        currentCollisions.Add(collider.gameObject);
    }

}