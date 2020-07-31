using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class ArrayDevManager : MonoBehaviour
{
    private static ArrayDevManager _instance;

    private bool tracking;
    private float devDistance;

    private Vector3 devPosition;
    private Vector3 devRotation;
    private Vector3 devVelocity;
    private Vector3 devAcceleration;
    private System.Diagnostics.Stopwatch stopWatch;

    private const int N = 8;
    private List<float> acceleration_X;

    public Text devInfoText;

    public static ArrayDevManager Instance { get { return _instance; } }

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
        ArrayDeviceGyro.InitGyro();
        devPosition = Vector3.zero;
        devRotation = Vector3.zero;
        devVelocity = Vector3.zero;
        devAcceleration = Vector3.zero;
        acceleration_X = new List<float>();
        stopWatch = new System.Diagnostics.Stopwatch();
    }

    private void Update()
    {
        // Update current rotation
        devRotation = ArrayDeviceGyro.ReadGyroscopeRotation().eulerAngles;

        if (tracking)
            EvaluateMovement();

        // Update current acceleration
        SampleAcceleration();

        // Update GUI
        devInfoText.text = "Device Info:\n Position: " + devPosition + "\n Rotation: " + devRotation + "\n AccelerationX: " + devAcceleration.x +
                           "\n AccelerationY: " + devAcceleration.y + "\n AccelerationZ: " + devAcceleration.z;
    }

    private void SampleAcceleration()
    {
        Vector3 values = Input.acceleration * 9.81f;
        Vector3 gravity = Input.gyro.gravity * 9.81f;

        if (tracking)
            stopWatch.Restart();

        devAcceleration.x = values.x - gravity.x;
        devAcceleration.y = values.y - gravity.y;
        devAcceleration.z = values.z - gravity.z;

        acceleration_X.Add(devAcceleration.x);
        if (acceleration_X.Count > N)
            acceleration_X.RemoveAt(0);
    }

    private void EvaluateMovement()
    {
        // Evaluate the device velocity and movement direction starting from the accelerometer and gyroscope info
        stopWatch.Stop();
        float dt = stopWatch.Elapsed.Milliseconds * 0.001f;

        float dx = 0.0f;
        float vx = 0.0f;
        for (int i = 1; i < N; i++)
        {
            //vx += acceleration_X[i] * dt; // With i starting from 0 in the for loop
            vx += (acceleration_X[i - 1] + acceleration_X[i]) / 2.0f * dt;
            dx += vx * dt;
        }
        devPosition.x += dx;
        devVelocity.x = vx;

        /*
        devPosition.x = devPosition.x + dt * devVelocity.x;
        devVelocity.x = devVelocity.x + dt * devAcceleration.x;
        */
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