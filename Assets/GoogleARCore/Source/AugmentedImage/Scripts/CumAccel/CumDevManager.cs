using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class CumDevManager : MonoBehaviour
{
    private static CumDevManager _instance;

    private bool tracking;
    private float devDistance;
    private Vector3 devPosition;
    private Vector3 devRotation;
    private Vector3 devAcceleration;

    private readonly float dt = 0.001f;
    private const float kFilteringFactor = 0.1f;
    private Vector3 accThreshold = new Vector3(0.08f, 0.08f, 0.08f);
    private Vector3 lastAccel;
    private Vector3 totalAccel;
    private Vector3 vel;
    private Vector3 initVel;
    private Vector3 distance;

    public Text devInfoText;
    private Stopwatch sw;
    private float currTimestamp;
    private float prevTimestamp;

    public static CumDevManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        tracking = false;
        devDistance = -1.0f;
        devPosition = Vector3.zero;
        devRotation = Vector3.zero;
        devAcceleration = Vector3.zero;

        lastAccel = Vector3.zero;
        totalAccel = Vector3.zero;
        vel = Vector3.zero;
        initVel = Vector3.zero;
        distance = Vector3.zero;

        CumDeviceGyro.InitGyro();
        sw = null;
        currTimestamp = 0.0f;
        prevTimestamp = 0.0f;
        InvokeRepeating("AccelerationSample", 0.0f, dt);
    }

    private void Update()
    {
        // Update current rotation
        devRotation = CumDeviceGyro.ReadGyroscopeRotation().eulerAngles;

        // Update current acceleration
        devAcceleration = Input.gyro.userAcceleration;

        // Update GUI
        devInfoText.text = "Device Info:\n Position: " + devPosition + "\n Rotation: " + devRotation + "\n FPS: " + 1.0f / Time.deltaTime +
                           "\n LastAccel: " + lastAccel + "\n TotAccel: " + totalAccel + "\n Velocity: " + vel + "\n DistanceX: " + distance.x;
    }

    private void AccelerationSample()
    {
        if (!tracking)
            return;
        if (sw == null)
        {
            sw = Stopwatch.StartNew();
            return;
        }

        currTimestamp = sw.ElapsedMilliseconds * 0.001f;
        Vector3 data = Input.gyro.userAcceleration * 9.81f; //Fetch the current acceleration
        lastAccel = data * kFilteringFactor + lastAccel * (1.0f - kFilteringFactor); //Apply Kalman filter
        totalAccel += data - lastAccel; //Accumulate the acceleration value

        vel = initVel + (totalAccel * (currTimestamp - prevTimestamp)); //Calculate the current velocity
        initVel = vel; //Update the initial velocity for the next frames

        distance += vel * (currTimestamp - prevTimestamp); //Calculate the distance from the velocity value

        /*
        if (totalAccel.x < accThreshold.x || totalAccel.x > -accThreshold.x)
        {
            vel.x = 0.0f;
            initVel.x = 0.0f;
        }
        if (totalAccel.y < accThreshold.y || totalAccel.y > -accThreshold.y)
        {
            vel.y = 0.0f;
            initVel.y = 0.0f;
        }
        if (totalAccel.z < accThreshold.z || totalAccel.z > -accThreshold.z)
        {
            vel.z = 0.0f;
            initVel.z = 0.0f;
        }
        */

        prevTimestamp = currTimestamp;
    }

    public void SetDistance(float dist)
    {
        if (devDistance == -1.0f && dist != 0)
        {
            SetPosition(0.0f, 0.0f, -dist);
            tracking = true;
        }
        else if (devDistance == 0.0f)
        {
            SetPosition(0.0f, 0.0f, 0.0f);
            devDistance = -1.0f;
            tracking = false;
            return;
        }

        devDistance = dist; // The calculated distance between the device and the marker
    }

    public void SetPosition(float x, float y, float z)
    {
        devPosition.x = x; // The alignment error between the device and the marker centres (horizontal axis)
        devPosition.y = y; // The height difference between the device and the marker centres (vertical axis)
        devPosition.z = z; // The linear distance between the device and the marker (forward axis)
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }
}