using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImuLogger : MonoBehaviour
{
    public SimulationSettings simuSetting;
    public ImuSensor imuSensor;
    public string imuFile = "imu0.csv";

    private List<ImuData> imuDataPoints;
    private string outputFolder;

    private struct ImuData
    {
        public ulong time;
        public float accX;
        public float accY;
        public float accZ;
        public float gyroX;
        public float gyroY;
        public float gyroZ;

        public string CsvFormat()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6}", time,
                gyroX * Mathf.Deg2Rad, gyroY * Mathf.Deg2Rad, gyroZ * Mathf.Deg2Rad,
                accX, accY, accZ);
        }

        public string CsvFormatRos()
        {
            Quaternion q = Quaternion.Euler(gyroX, gyroY, gyroZ);
            Quaternion rosQ = new Quaternion(-q.z, q.x, -q.y, q.w);
            Vector3 rosAngles = rosQ.eulerAngles;
            FormatAngle(ref rosAngles);
            rosAngles *= Mathf.Deg2Rad;
            Debug.Log(string.Format("{0},{1},{2},{3},{4},{5},{6}", time,
                rosAngles.x, rosAngles.y, rosAngles.z,
                accZ, -accX, accY));
            return string.Format("{0},{1},{2},{3},{4},{5},{6}", time,
                rosAngles.x, rosAngles.y, rosAngles.z,
                accZ, -accX, accY);
        }

        private static void FormatAngle(ref Vector3 angles)
        {
            // WARNING: works only if delta angle < 180° per frame (<9000°/s @ 50Hz).
            if (angles.x > 180f)
            {
                angles.x = -(360f - angles.x);
            }
            if (angles.y > 180f)
            {
                angles.y = -(360f - angles.y);
            }
            if (angles.z > 180f)
            {
                angles.z = -(360f - angles.z);
            }
        }
    }
    
    void Awake()
    {
        imuDataPoints = new List<ImuData>();

        string folder = "imu_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
        outputFolder = Path.Combine(simuSetting.dataOutputDirectory, folder);
        DirectoryInfo di = Directory.CreateDirectory(outputFolder);
    }

    private void OnDestroy()
    {
        WriteDataPoint(imuFile, imuDataPoints);
    }

    void Update()
    {
        if (simuSetting.motionAndSensingUpdateScheme == SimulationSettings.UpdateScheme.renderUpdate)
        {
            imuSensor.GetImuData(out Vector3 acc, out Vector3 gyro, out float time);
            Debug.Log(string.Format("Acc {0} | Gyro {1} @ {2}", acc, gyro, time * 1e9));
            AddDataPoint(time, acc, gyro);
        }
    }
    float prevT = 0;
    void FixedUpdate()
    {
        if (simuSetting.motionAndSensingUpdateScheme == SimulationSettings.UpdateScheme.physxUpdate)
        {
            imuSensor.GetImuData(out Vector3 acc, out Vector3 gyro, out float time);
            Debug.Log(string.Format("Acc {0} | Gyro {1} @ {2}", acc, gyro, time * 1e3));

            float newT = Time.realtimeSinceStartup;
            Debug.Log("dt IMU: " + ((newT - prevT) * 1e3));
            prevT = newT;
            AddDataPoint(time, acc, gyro);
        }
    }

    private void AddDataPoint(float time, Vector3 acc, Vector3 gyro)
    {
        imuDataPoints.Add(
            new ImuData()
            {
                time = (ulong)(time * 1e9), // nano seconds
                accX = acc.x,
                accY = acc.y,
                accZ = acc.z,
                gyroX = gyro.x,
                gyroY = gyro.y,
                gyroZ = gyro.z
            }
        );
    }

    private bool WriteDataPoint(string imuFile, List<ImuData> dataPoints)
    {
        string imuFilePath = Path.Combine(outputFolder, imuFile);
        try
        {
            using (StreamWriter outputFile = new StreamWriter(imuFilePath))
            {
                foreach (var dataPoint in dataPoints)
                    outputFile.WriteLine(dataPoint.CsvFormatRos());
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
