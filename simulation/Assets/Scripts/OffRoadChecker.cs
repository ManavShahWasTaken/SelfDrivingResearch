using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OffRoadChecker : MonoBehaviour
{
    //private CarController carController;
    public CarController player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<CarController>();
    }
    // Detects if the car has left the track
    void OnTriggerExit(Collider collider)
    {
        print("Checker: " + collider.gameObject.name + ", " + collider.gameObject.tag);
        if (collider.gameObject.tag == "Track")
        {
            // player.resetPosition();
            bool crashed = true;
            player.SendValue();

        }
    }
}