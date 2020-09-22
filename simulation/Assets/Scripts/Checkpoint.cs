using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Represents if the current checkpoint is activated or not.
    public bool activated = false;

    // All checkpoints in the scene
    public static GameObject[] CheckPointsList;

    private static int current_checkpoint = 0;

    // Start is called before the first frame update
    void Start()
    {
        int num_checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint").Length;
        GameObject[] tempCheckPointsList = new GameObject[num_checkpoints];
        for(int i = 1; i <= num_checkpoints; i++){
            tempCheckPointsList[i-1] = GameObject.Find("/Checkpoints/Checkpoint" + i);
        }
        CheckPointsList = tempCheckPointsList;
    }

    // Activates the next checkpoint for the agent to reach.
    private void activateNextCheckpoint()
    {
        if(current_checkpoint + 1 == CheckPointsList.Length)
        {
            current_checkpoint = 0;
        }
        else
        {
            current_checkpoint++;
        }
    }

    // If the player passes through the checkpoint, we activate it
    void OnTriggerEnter(Collider entity)
    {      
        if (entity.tag == "Player")
        {
            activateNextCheckpoint();
        }
    }

    // Returns the Position of the current checkpoint the agent is trying to reach.
    public static Vector3 getCurrentCheckpointLocation()
    {
        return CheckPointsList[current_checkpoint].transform.position;
    }

    // Returns a random Checkpoint on the track.
    public static GameObject getRandomCheckpoint()
    {
        int index = Random.Range(0,CheckPointsList.Length);
        return CheckPointsList[index];
    }

    public static void setCurrentCheckPoint(GameObject newCheckpoint)
    {
        current_checkpoint = System.Array.IndexOf(CheckPointsList, newCheckpoint);
    }
}
