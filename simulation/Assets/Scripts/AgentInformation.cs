using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentInformation : MonoBehaviour
{

    public Text gui_info;


    protected double velocity;
    protected double angle_from_road;
    protected double distance_from_road;

    private CarController carController;

    // private Checkpoint checkpoint_info;
    private GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        velocity = 0;
        angle_from_road = 0;
        distance_from_road = 0;
        player = GameObject.FindGameObjectsWithTag("Player")[0];

        // Added line to get information out - Sobi
        carController = GameObject.FindObjectOfType<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        velocity = System.Math.Round(player.GetComponent<Rigidbody>().velocity.magnitude, 2);
        Vector3 checkpoint_location = Checkpoint.getCurrentCheckpointLocation();
        Vector3 vector_to_checkpoint = checkpoint_location - player.transform.position;
        distance_from_road = System.Math.Round(vector_to_checkpoint.magnitude, 2);
        
        angle_from_road = System.Math.Round(Vector3.Angle(vector_to_checkpoint, player.transform.forward) * Mathf.Deg2Rad, 2);

        // Sends the data array to CarController Script - Sobi

        double[] data = {angle_from_road, distance_from_road, velocity};
        carController.UpdateValues(data);

        


        // Remove this line to disable GUI
        gui_info.text = "Speed: " + velocity + "\nDistance: " + distance_from_road + "\nAngle: " + angle_from_road;
        
    }
}
