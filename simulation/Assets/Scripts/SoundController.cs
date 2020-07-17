using UnityEngine;
using System.Collections;

// Simple class to control sounds of the car, based on engine throttle and RPM, and skid velocity.
[RequireComponent(typeof(Drivetrain))]
[RequireComponent(typeof(CarController))]
public class SoundController : MonoBehaviour {

    public AudioClip engine1;
    //public AudioClip engine2;
    public AudioClip skid;
    public AudioClip shiftUp;
    public AudioClip shiftDown;
    public AudioClip blowOffValve;
    public AudioClip[] transmission;
    public AudioClip[] backfire;

    AudioSource engineSource1;
    AudioSource engineSource2;
    AudioSource skidSource;
    AudioSource shiftUpSource;
    AudioSource shiftDownSource;
    AudioSource blowOffValveSource;
    AudioSource transmissionOnSource;
    AudioSource transmissionOffSource;
    public AudioSource backfireSource;

    CarController car;
    Drivetrain drivetrain;

    AudioSource CreateAudioSource(AudioClip clip, string name) {
        GameObject go = new GameObject(name);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.AddComponent(typeof(AudioSource));
        go.GetComponent<AudioSource>().clip = clip;
        go.GetComponent<AudioSource>().loop = true;
        go.GetComponent<AudioSource>().volume = 0.20f;
        go.GetComponent<AudioSource>().spatialBlend = 1f;
        go.GetComponent<AudioSource>().dopplerLevel = 0f;
        go.GetComponent<AudioSource>().Play();
        return go.GetComponent<AudioSource>();
    }

    AudioSource CreateAudioSourceShift(AudioClip clip, string name) {
        GameObject go = new GameObject(name);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.AddComponent(typeof(AudioSource));
        go.GetComponent<AudioSource>().clip = clip;
        go.GetComponent<AudioSource>().loop = false;
        go.GetComponent<AudioSource>().volume = 0.75f;
        go.GetComponent<AudioSource>().spatialBlend = 1f;
        go.GetComponent<AudioSource>().dopplerLevel = 0f;
        return go.GetComponent<AudioSource>();
    }

    void Start() {
    }

    public void playShiftUp() {
        // shiftUpSource.Play();
    }

    public void playShiftDown() {
        // shiftDownSource.Play();
    }

    public void playBOV() {
        // if ((drivetrain.rpm / drivetrain.maxRPM) > 0.80f)
            // blowOffValveSource.Play();
    }

    public void playBackFire() {
        // backfireSource.clip = backfire[Random.Range(0, 3)];
        // backfireSource.Play();
    }

    // int currSpeed, lastSpeed, difference;

    void Update() {
    }
}