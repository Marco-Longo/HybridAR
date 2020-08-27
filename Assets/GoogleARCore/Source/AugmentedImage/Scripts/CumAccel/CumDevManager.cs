using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class CumDevManager : MonoBehaviour
{
    private static CumDevManager _instance;

    private bool tracking;
    private string dataString;
    private float devDistance;
    private Vector3 devPosition;
    private Vector3 devRotation;
    private Vector3 devAcceleration;

    private readonly float dt = 0.001f;
    private const float kFilteringFactor = 0.1f;
    private Vector3 lastAccel;
    private Vector3 totalAccel;
    private Vector3 vel;
    private Vector3 initVel;
    
    public Text devInfoText;
    public Text debugText;
    public Image outputEntry1;
    public Image outputEntry2;
    public Image outputEntry3;
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
        dataString = "CumulativeAccel\n";
        devDistance = -1.0f;
        devPosition = Vector3.zero;
        devRotation = Vector3.zero;
        devAcceleration = Vector3.zero;

        lastAccel = Vector3.zero;
        totalAccel = Vector3.zero;
        vel = Vector3.zero;
        initVel = Vector3.zero;

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
        devInfoText.text = "Device Info:\n Position: " + devPosition + "\n Rotation: " + devRotation + "\n Acceleration: " + lastAccel +
                           "\n Velocity: " + vel;
        outputEntry1.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "X: " + devPosition.x.ToString();
        outputEntry2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Y: " + devPosition.y.ToString();
        outputEntry3.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Z: " + devPosition.z.ToString();
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
        if (lastAccel.magnitude < 0.1f)
            vel = Vector3.zero;
        initVel = vel; //Update the initial velocity for the next frames

        devPosition += vel * (currTimestamp - prevTimestamp); //Calculate the distance from the velocity value
        
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

    public void ToggleDebug()
    {
        devInfoText.gameObject.SetActive(!devInfoText.gameObject.activeInHierarchy);
        debugText.gameObject.SetActive(!debugText.gameObject.activeInHierarchy);
        outputEntry1.gameObject.SetActive(!outputEntry1.gameObject.activeInHierarchy);
        outputEntry2.gameObject.SetActive(!outputEntry2.gameObject.activeInHierarchy);
        outputEntry3.gameObject.SetActive(!outputEntry3.gameObject.activeInHierarchy);
    }

    public void RegisterData()
    {
        dataString += devPosition.x.ToString() + ",";
        dataString += devPosition.y.ToString() + ",";
        dataString += devPosition.z.ToString();
        dataString += "\n";

        PlayerPrefs.SetString("FILE", dataString);
    }
}