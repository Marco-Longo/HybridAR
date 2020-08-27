using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleARCore;
using System;

public class ARDeviceManager : MonoBehaviour
{
    private static ARDeviceManager _instance;
    private float devDistance;
    private bool tracking;
    private string dataString;

    private Vector3 devPosition;
    private Vector3 devOffset;
    private Vector3 devRotation;
    private Vector3 devAcceleration;

    public Text devInfoText;
    public Text debugText;

    public static ARDeviceManager Instance { get { return _instance; } }

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
        dataString = "ARCoreApproach\n";
        devDistance = -1.0f;
        devPosition = Vector3.zero;
        devOffset = Vector3.zero;
        devRotation = Vector3.zero;
        devAcceleration = Vector3.zero;

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = 0.0167f;
        }
    }

    private void Update()
    {
        // Update current position
        if (tracking)
            EvaluatePosition();

        // Update current rotation
        devRotation = ReadGyroscopeRotation().eulerAngles;

        // Update current acceleration
        devAcceleration = Input.gyro.userAcceleration;

        devInfoText.text = "Device Info:\n Position: " + devPosition + "\n Rotation: " + devRotation + "\n Acceleration: " + devAcceleration +
                           "\n Camera Pose: " + Frame.Pose.ToString();
    }

    private void EvaluatePosition()
    {
        if (devOffset == Frame.Pose.position)
            return;

        //Evaluate the device position, while considering the initial offset
        devPosition += Frame.Pose.position - devOffset;
        devOffset = Frame.Pose.position;
    }

    private Quaternion ReadGyroscopeRotation()
    {
        if (Input.gyro.enabled)
            return new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
        else
            return Quaternion.identity;
    }

    public void SetDistance(float d)
    {
        if (devDistance == -1.0f && d > 0.0f)
        {
            //Calculate initial position
            devPosition = new Vector3(0.0f, 0.0f, -d); //We assume that the device is perfectly aligned to the marker (origin of the system)
            devOffset = Frame.Pose.position;
            tracking = true;
        }

        devDistance = d;
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ToggleDebug()
    {
        devInfoText.gameObject.SetActive(!devInfoText.gameObject.activeInHierarchy);
        debugText.gameObject.SetActive(!debugText.gameObject.activeInHierarchy);
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