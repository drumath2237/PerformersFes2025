using Microsoft.Azure.Kinect.Sensor;
using UnityEngine;

namespace PerformersFes
{
    public class KinectTest : MonoBehaviour
    {
        private void Start()
        {
            using var kinect = Device.Open();
            Debug.Log(kinect.Version.DepthSensor.ToString());
        }
    }
}