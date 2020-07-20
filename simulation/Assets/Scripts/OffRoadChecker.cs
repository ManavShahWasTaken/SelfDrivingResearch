using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffRoadChecker : MonoBehaviour
{
    public CarController player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<CarController>();
    }
    // Detects if the car has left the track
    void OnCollisionExit(Collision collision)
    {
        print(collision.gameObject.name);
        if (collision.gameObject.name == "Track")
        {
            player.resetPosition();
        }
    }
    // // Update is called once per frame
    // void Update()
    // {
    // }
}