using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCarDerailing : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0];
    }

    void OnTriggerExit(Collider collider){
        if (collider.gameObject.tag == "Player"){
            GameObject checkpoint = Checkpoint.getRandomCheckpoint();
            print(checkpoint);
            // Transform parent = this.transform.parent;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            player.transform.position = checkpoint.transform.position;
            player.transform.rotation = checkpoint.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
