using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

public class Server
{

    static Int32 port = 4444;
    static TcpClient client;
    static NetworkStream stream;

    public static void ConnectToAgent()
    {
        // Debug.Log("Connecting to Agent...");
        client = new TcpClient("127.0.0.1", port);
        stream = client.GetStream();
        // Debug.Log("Connected to Agent!");

    }

    // tested
    public static string WaitForString()
    {
        Byte[] data = new Byte[32];
        Int32 bytes = stream.Read(data, 0, data.Length); // blocking line
        stream.Flush();
        return System.Text.Encoding.ASCII.GetString(data, 0, bytes);
    }
    // tested
    public static void SendMessage(string message)
    {
        Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
        stream.Flush();
    }
}



// This class is repsonsible for controlling inputs to the car.
[RequireComponent(typeof(Drivetrain))]
public class CarController : MonoBehaviour
{

    // usefull initialisations
    int timeSteps;
    int tempTimeSteps; // records how many frames/timesteps have passed since the latest action was recieved.
    int stepsPerAction; // we perform stepsPerAction number of steps for with a given action and then communicate the reward and observation
                        // back to the agent. The calculation of reward and sending of observation(screenshots and speed, angle and distance to checkpoint)
                        // still needs to be implemented
    char[] actions;

    bool crashed;



    // Add all wheels of the car here, so brake and steering forces can be applied to them.
    public Wheel[] wheels;

    // A transform object which marks the car's center of gravity.
    // Cars with a higher CoG tend to tilt more in corners.
    // The further the CoG is towards the rear of the car, the more the car tends to oversteer. 
    // If this is not set, the center of mass is calculated from the colliders.
    public Transform centerOfMass;

    // A factor applied to the car's inertia tensor. 
    // Unity calculates the inertia tensor based on the car's collider shape.
    // This factor lets you scale the tensor, in order to make the car more or less dynamic.
    // A higher inertia makes the car change direction slower, which can make it easier to respond to.
    public float inertiaFactor = 1.5f;

    // current input state
    [HideInInspector]
    public float brake;
    [HideInInspector]
    float throttle;
    float throttleInput;
    float clutch;
    [HideInInspector]
    public float steering;
    float lastShiftTime = -1;
    [HideInInspector]
    public float handbrake;

    // cached Drivetrain reference
    Drivetrain drivetrain;

    // How long the car takes to shift gears
    public float shiftSpeed = 0.8f;


    // These values determine how fast throttle value is changed when the accelerate keys are pressed or released.
    // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.
    // There are different values for when the wheels have full traction and when there are spinning, to implement 
    // traction control schemes.

    // How long it takes to fully engage the throttle
    public float throttleTime = 1.0f;
    // How long it takes to fully engage the throttle 
    // when the wheels are spinning (and traction control is disabled)
    public float throttleTimeTraction = 10.0f;
    // How long it takes to fully release the throttle
    public float throttleReleaseTime = 0.5f;
    // How long it takes to fully release the throttle 
    // when the wheels are spinning.
    public float throttleReleaseTimeTraction = 0.1f;

    // Turn traction control on or off
    public bool tractionControl = false;

    // Turn ABS control on or off
    public bool absControl = false;

    // These values determine how fast steering value is changed when the steering keys are pressed or released.
    // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.

    // How long it takes to fully turn the steering wheel from center to full lock
    public float steerTime = 1.2f;
    // This is added to steerTime per m/s of velocity, so steering is slower when the car is moving faster.
    public float veloSteerTime = 0.1f;

    // How long it takes to fully turn the steering wheel from full lock to center
    public float steerReleaseTime = 0.6f;
    // This is added to steerReleaseTime per m/s of velocity, so steering is slower when the car is moving faster.
    public float veloSteerReleaseTime = 0f;
    // When detecting a situation where the player tries to counter steer to correct an oversteer situation,
    // steering speed will be multiplied by the difference between optimal and current steering times this 
    // factor, to make the correction easier.
    public float steerCorrectionFactor = 4.0f;

    private bool gearShifted = false;
    private bool gearShiftedFlag = false;

    // Used by SoundController to get average slip velo of all wheels for skid sounds.
    public float slipVelo
    {
        get
        {
            float val = 0.0f;
            foreach (Wheel w in wheels)
                val += w.slipVelo / wheels.Length;
            return val;
        }

    }

    // This variable holds the 3 data values from AgentInformation.
    public double[] input = new double[3];

    //The following method obtains the 3 values from AgentInformation in the form of an Array.
    public void UpdateValues(double[] input_data)
    {
        input = input_data;
    }

    // Resets the car's position to a random checkpoint on the track.
    public void resetPosition()
    {
        GameObject checkpoint = Checkpoint.getRandomCheckpoint();
        // Transform parent = this.transform.parent;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        this.transform.position = checkpoint.transform.position;
        this.transform.rotation = checkpoint.transform.rotation;
        Checkpoint.setCurrentCheckPoint(checkpoint);
    }


    // Detects if the car has left the track
    void OnCollisionExit(Collision collision)
    {
        print(collision.gameObject.name);
        if (String.Equals(collision.gameObject.name, "Track"))
        {
            this.crashed = true;
            // resetPosition();
        }
    }

    // Detects if the car has left the track
    void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        if (!String.Equals(collision.gameObject.tag, "Track"))
        {
            this.crashed = true;
            // resetPosition();
        }
    }


    void Pause()
    {
        Time.timeScale = 0f;
    }

    void Unpause()
    {
        Time.timeScale = 1f;
    }

    // Initialize
    void Start()
    {	
	    //resetPosition();
        if (centerOfMass != null)
            GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;

        GetComponent<Rigidbody>().inertiaTensor *= inertiaFactor;
        drivetrain = GetComponent(typeof(Drivetrain)) as Drivetrain;
        this.timeSteps = 0;
        this.tempTimeSteps = 0;
        this.stepsPerAction = 4;
        this.crashed = false;
        this.actions = new char[2];
        this.actions[0] = 'd';
        this.actions[1] = 's';
        Server.ConnectToAgent();
    }

    void Update()
    {
        if (this.tempTimeSteps == 5)
        {
            print("here");
        }
        // send info when time step ends.
        if (this.tempTimeSteps >= this.stepsPerAction || this.crashed)
        {
            // SOBI WORK FOR AGENT INFORMATION
            string angle_from_centre = input.GetValue(0).ToString();
            string distance = input.GetValue(1).ToString();
            string speed = input.GetValue(2).ToString();
            string crash_value;
            if (this.crashed) crash_value = "1";
            else crash_value = "0";
            string frames_captured = this.tempTimeSteps.ToString();
            string infoToSend = angle_from_centre + ", " + distance + ", " + speed + ", " + crash_value + ", " + frames_captured;
            Server.SendMessage(infoToSend);
            this.tempTimeSteps = 0;
        }
        // Wait for action if temp step is starting out
        if (this.tempTimeSteps == 0)
        {
            // receive that action as a string
            String temp = Server.WaitForString();
            // print(temp);
            if (temp == "reset")
            {
                // if the action is to reset, reset the position and set the action to do nothing for 4 frames so that we get 4 screenshots in.
                temp = "ds";
                if (this.timeSteps > 1)
                {
                    resetPosition();
                }
                this.timeSteps = 0;
                this.tempTimeSteps = 0;
                this.crashed = false;
		        
            }
            this.actions = temp.ToCharArray();
        }

        this.tempTimeSteps++;
        this.timeSteps++;

        // Steering
        Vector3 carDir = transform.forward;
        float fVelo = GetComponent<Rigidbody>().velocity.magnitude;
        Vector3 veloDir = GetComponent<Rigidbody>().velocity * (1 / fVelo);
        float angle = -Mathf.Asin(Mathf.Clamp(Vector3.Cross(veloDir, carDir).y, -1, 1));
        float optimalSteering = angle / (wheels[0].maxSteeringAngle * Mathf.Deg2Rad);
        if (fVelo < 1)
            optimalSteering = 0;

        float steerInput = 0;

        // String will be a in the form of 'ef', where e is the acceleration action and f is the steering action.
        // Acceleration can have commands for b (brake), d (do nothing), a (accelerate) and steering will feature l (left), s (straight), r (right)

        char linear = this.actions[0];
        char steeringControl = this.actions[1];

        // The following code will instruct agent to turn left, right or straight depending on command string
        if (steeringControl == 'l')
        {
            steerInput = -1;
        }
        else if (steeringControl == 'r')
        {
            steerInput = 1;
        }
        else if (steeringControl == 's')
        {
            steerInput = 0;
        }

        // ignore 
        if (steerInput < steering)
        {
            float steerSpeed = (steering > 0) ? (1 / (steerReleaseTime + veloSteerReleaseTime * fVelo)) : (1 / (steerTime + veloSteerTime * fVelo));
            if (steering > optimalSteering)
                steerSpeed *= 1 + (steering - optimalSteering) * steerCorrectionFactor;
            steering -= steerSpeed * Time.deltaTime;
            if (steerInput > steering)
                steering = steerInput;
        }
        else if (steerInput > steering)
        {
            float steerSpeed = (steering < 0) ? (1 / (steerReleaseTime + veloSteerReleaseTime * fVelo)) : (1 / (steerTime + veloSteerTime * fVelo));
            if (steering < optimalSteering)
                steerSpeed *= 1 + (optimalSteering - steering) * steerCorrectionFactor;
            steering += steerSpeed * Time.deltaTime;
            if (steerInput < steering)
                steering = steerInput;
        }
        // =======


        // Change this value to see if the car must accelerate this frame or not.
        bool accelKey = linear == 'a';

        // Change this value to see if the car must brake this frame or not.
        bool brakeKey = linear == 'b';


        if (drivetrain.automatic && drivetrain.gear == 0)
        {
            accelKey = accelKey; // Input.GetKey(KeyCode.DownArrow);
            brakeKey = brakeKey; // Input.GetKey(KeyCode.UpArrow);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            throttle += Time.deltaTime / throttleTime;
            throttleInput += Time.deltaTime / throttleTime;
        }
        else if (accelKey)
        {

            if (drivetrain.slipRatio < 0.10f)
                throttle += Time.deltaTime / throttleTime;
            else if (!tractionControl)
                throttle += Time.deltaTime / throttleTimeTraction;
            else
                throttle -= Time.deltaTime / throttleReleaseTime;

            if (throttleInput < 0)
                throttleInput = 0;
            throttleInput += Time.deltaTime / throttleTime;
        }
        else
        {
            if (drivetrain.slipRatio < 0.2f)
                throttle -= Time.deltaTime / throttleReleaseTime;
            else
                throttle -= Time.deltaTime / throttleReleaseTimeTraction;
        }

        throttle = Mathf.Clamp01(throttle);

        if (brakeKey)
        {
            if (drivetrain.slipRatio < 0.2f)
                brake += Time.deltaTime / throttleTime;
            else
                brake += Time.deltaTime / throttleTimeTraction;
            throttle = 0;
            throttleInput -= Time.deltaTime / throttleTime;
        }
        else
        {
            if (drivetrain.slipRatio < 0.2f)
                brake -= Time.deltaTime / throttleReleaseTime;
            else
                brake -= Time.deltaTime / throttleReleaseTimeTraction;
        }

        brake = Mathf.Clamp01(brake);
        throttleInput = Mathf.Clamp(throttleInput, -1, 1);

        // Handbrake
        handbrake = (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton2)) ? 1f : 0f;

        // Gear shifting
        float shiftThrottleFactor = Mathf.Clamp01((Time.time - lastShiftTime) / shiftSpeed);

        if (drivetrain.gear == 0 && Input.GetKey(KeyCode.UpArrow))
        {
            throttle = 0.4f;// Anti reverse lock thingy??
        }

        if (drivetrain.gear == 0)
            drivetrain.throttle = accelKey ? throttle : 0f;
        else
            drivetrain.throttle = accelKey ? (tractionControl ? throttle : 1) * shiftThrottleFactor : 0f;

        drivetrain.throttleInput = throttleInput;

        if (Input.GetKeyDown(KeyCode.A))
        {
            lastShiftTime = Time.time;
            drivetrain.ShiftUp();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            lastShiftTime = Time.time;
            drivetrain.ShiftDown();
        }

        //play gear shift sound
        if (gearShifted && gearShiftedFlag && drivetrain.gear != 1)
        {
            GetComponent<SoundController>().playShiftUp();
            gearShifted = false;
            gearShiftedFlag = false;
        }


        // ABS Trigger (This prototype version is used to prevent wheel lock , currently expiremental)
        if (absControl)
            brake -= brake >= 0.1f ? 0.1f : 0f;

        // Apply inputs
        foreach (Wheel w in wheels)
        {
            w.brake = brakeKey ? brake : 0;
            w.handbrake = handbrake;
            w.steering = steering;
        }

        // Reset Car position and rotation in case it rolls over
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            transform.rotation = Quaternion.Euler(0, transform.localRotation.y, 0);
        }


        // Traction Control Toggle
        if (Input.GetKeyDown(KeyCode.T))
        {

            if (tractionControl)
            {
                tractionControl = false;
            }
            else
            {
                tractionControl = true;
            }
        }

        // Anti-Brake Lock Toggle
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (absControl)
            {
                absControl = false;
            }
            else
            {
                absControl = true;
            }
        }
        
    }

    public string ScreenShotName()
    {
        string temp = this.tempTimeSteps.ToString();
        if(this.tempTimeSteps == 0)
        {
            temp = "1";
        }
        string str = "Screenshots/scr" + temp + ".png";
        return str;
    }

    void LateUpdate()
    {
        print(ScreenShotName());
        // if (this.tempTimeSteps >= 1 && this.tempTimeSteps <= 4)
        ScreenCapture.CaptureScreenshot(ScreenShotName());
    }

    // // Debug GUI. Disable when not needed.
    // void OnGUI() {
    //     GUI.Label(new Rect(0, 60, 100, 200), "km/h: " + GetComponent<Rigidbody>().velocity.magnitude * 3.6f);
    //     tractionControl = GUI.Toggle(new Rect(0, 80, 300, 20), tractionControl, "Traction Control (bypassed by shift key)");
    // }
}