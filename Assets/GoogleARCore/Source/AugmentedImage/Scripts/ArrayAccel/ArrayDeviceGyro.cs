using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayDeviceGyro
{
    private static bool gyroInitialized = false;

    public static bool HasGyroscope
    {
        get
        {
            return SystemInfo.supportsGyroscope;
        }
    }

    public static Quaternion Get()
    {
        if (!gyroInitialized)
        {
            InitGyro();
        }

        return HasGyroscope
            ? ReadGyroscopeRotation()
            : Quaternion.identity;
    }

    public static void InitGyro()
    {
        if (HasGyroscope)
        {
            Input.gyro.enabled = true;              // enable the gyroscope
            Input.gyro.updateInterval = 0.0334f;    // set the update interval to it's highest value (30 Hz)
            //Input.gyro.updateInterval = 0.00833f;
        }
        gyroInitialized = true;
    }

    public static Quaternion ReadGyroscopeRotation()
    {
        return new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
    }
}